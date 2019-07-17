using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Giselle.Commons;
using Giselle.DoujinshiDownloader.Configs;
using Giselle.DoujinshiDownloader.Doujinshi;
using Giselle.DoujinshiDownloader.Forms;
using Giselle.DoujinshiDownloader.Forms.Utils;
using Giselle.DoujinshiDownloader.Resources;
using Giselle.DoujinshiDownloader.Schedulers;
using Giselle.DoujinshiDownloader.Utils;

namespace Giselle.DoujinshiDownloader
{
    public class DoujinshiDownloader : IDisposable
    {
        public static string FullName => "Giselle.DoujinshiDownloader";
        public static string Name => "DoujinshiDownloader";

        public static DoujinshiDownloader Instance { get; private set; } = null;

        [STAThread]
        public static void Main(string[] args)
        {
            var result = CommandLine.Parser.Default.ParseArguments<CommandLineOptions>(args);

            if (result is CommandLine.NotParsed<CommandLineOptions>)
            {
                return;
            }
            else if (result is CommandLine.Parsed<CommandLineOptions> parsed)
            {
                using (var mutex = new Mutex(true, FullName, out var createdNew))
                {
                    if (createdNew == true)
                    {
                        var instance = Instance = new DoujinshiDownloader(parsed.Value);
                        instance.Run();
                    }
                    else
                    {
                        NativeMethods.PostMessage((IntPtr)NativeMethods.HWND_BROADCAST, NativeMethods.WM_ShowSingleInstance, IntPtr.Zero, IntPtr.Zero);
                    }

                }

            }

        }

        public ConfigurationManager Config { get; }
        public FontManager FontManager { get; }
        public NotifyIconManager NotifyIconManager { get; }
        public DownloadScheduler Scheduler { get; }

        public MainForm MainForm { get; private set; }
        public event EventHandler MainFormVisibleChanged;

        public DoujinshiDownloader(CommandLineOptions options)
        {
            var culture = options.Language.Execute(l => CultureInfo.GetCultureInfo(l)) ?? CultureInfo.CurrentUICulture;
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
            LanguageResource.Culture = culture;

            Console.WriteLine($"UI Language : {culture}");

            Console.CancelKeyPress += this.OnConsoleCancelKeyPress;
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            FormUtils.DefaultIcon = Properties.Resources.Icon;

            this.Config = new ConfigurationManager(PathUtils.GetPath("Configuration.json"));
            this.FontManager = new FontManager();
            this.NotifyIconManager = new NotifyIconManager(this);
            this.Scheduler = new DownloadScheduler();

            this.MainForm = null;
        }

        private void OnConsoleCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            ObjectUtils.DisposeQuietly(this);
        }

        private void Run()
        {
            try
            {
                this.Config.Load();

                this.Scheduler.Start();

                using (var mainForm = new MainForm())
                {
                    this.MainForm = mainForm;

                    mainForm.VisibleChanged += (sender, e) => this.OnMainFormVisibleChanged(e);
                    Application.Run(mainForm);
                }

            }
            catch (Exception e)
            {
                this.ShowCrashMessageBox(e);
            }
            finally
            {
                this.Dispose();
            }

        }

        public void ShowCrashMessageBox(Exception exception)
        {
            var file = this.DumpCrashMessage(exception);

            ControlUtils.InvokeIfNeed(this.MainForm, () =>
            {
                using (var form = new CrashReportForm(file, exception))
                {
                    form.ShowDialog();
                }

            });

        }

        protected virtual void OnMainFormVisibleChanged(EventArgs e)
        {
            this.MainFormVisibleChanged?.Invoke(this, e);
        }

        public string GetCrashReportsDirectory()
        {
            var directory = PathUtils.GetPath("CrashReports");
            Directory.CreateDirectory(directory);

            return directory;
        }

        public string DumpCrashMessage(Exception exception)
        {
            try
            {
                Console.WriteLine(exception);
                var directory = this.GetCrashReportsDirectory();

                var dateTime = DateTime.Now;
                var file = PathUtils.GetFilePathNotDuplicate(PathUtils.GetPath(directory, dateTime.ToString("yyyy_MM_dd HH_mm_ss_fff") + ".log"));

                File.WriteAllText(file, string.Concat(exception));

                return file;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }

        }

        public void QueryQuit()
        {
            var dd = DoujinshiDownloader.Instance;
            var scheduler = dd.Scheduler;
            var tasks = scheduler.GetQueueCopy();

            string text = null;

            if (scheduler.Busy == true || tasks.Count > 0)
            {
                text = $"진행중인 다운로드가 있습니다.{Environment.NewLine}정말로 종료하시겠습니까?";
            }
            else
            {
                text = "프로그램을 종료하시겠습니까?";
            }

            var result = MessageBox.Show(text, "종료 확인", MessageBoxButtons.OKCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);

            if (result == DialogResult.OK)
            {
                ObjectUtils.DisposeQuietly(this);
            }

        }

        ~DoujinshiDownloader()
        {
            this.Dispose(false);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            ObjectUtils.DisposeQuietly(this.MainForm);
            ObjectUtils.DisposeQuietly(this.Scheduler);
            ObjectUtils.DisposeQuietly(this.NotifyIconManager);
            ObjectUtils.DisposeQuietly(this.FontManager);
        }

    }

}
