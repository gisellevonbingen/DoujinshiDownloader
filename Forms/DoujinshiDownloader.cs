using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Giselle.Commons;
using Giselle.DoujinshiDownloader.Configs;
using Giselle.DoujinshiDownloader.Forms;
using Giselle.DoujinshiDownloader.Schedulers;
using Giselle.DoujinshiDownloader.Utils;
using Giselle.Forms;

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
                var options = parsed.Value;

                if (options.Console == true)
                {
                    NativeMethods.AllocConsole();
                }

                if (options.MultiInstance == true)
                {
                    var instance = new DoujinshiDownloader(options);
                    instance.Run();
                }
                else
                {
                    using (var mutex = new Mutex(true, FullName, out var createdNew))
                    {
                        if (createdNew == true)
                        {
                            var instance = new DoujinshiDownloader(options);
                            instance.Run();
                        }
                        else
                        {
                            NativeMethods.PostMessage((IntPtr)NativeMethods.HWND_BROADCAST, NativeMethods.WM_ShowSingleInstance, IntPtr.Zero, IntPtr.Zero);
                        }

                    }

                }

            }

        }

        public CommandLineOptions CommandLineOptions { get; }
        public ResourceManager ResourceManager { get; }
        public ConfigurationManager Config { get; }
        public NotifyIconManager NotifyIconManager { get; }
        public DownloadScheduler Scheduler { get; }

        public MainForm MainForm { get; private set; }
        public event EventHandler MainFormVisibleChanged;

        public DoujinshiDownloader(CommandLineOptions options)
        {
            Instance = this;

            this.CommandLineOptions = options;
            this.ResourceManager = new ResourceManager("Giselle.DoujinshiDownloader.Resources.LanguageResource", typeof(DoujinshiDownloader).Assembly);
            this.SetUILanguage(options.Language);

            Console.CancelKeyPress += this.OnConsoleCancelKeyPress;
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            FormUtils.DefaultIcon = Properties.Resources.Icon;

            this.Config = new ConfigurationManager(PathUtils.GetPath("Configuration.json"));
            this.NotifyIconManager = new NotifyIconManager();
            this.Scheduler = new DownloadScheduler();

            this.MainForm = null;
        }

        private void SetUILanguage(string language)
        {
            var culture = language.ConsumeSelect(l => CultureInfo.GetCultureInfo(l), CultureInfo.CurrentUICulture);
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;

            Console.WriteLine($"Culture : {culture.DisplayName}");
            Console.WriteLine($"UI Language : {this.ResourceManager.GetString("Type")}");
        }

        private void OnConsoleCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            this.DisposeQuietly();
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
            Console.WriteLine(exception);

            var file = this.DumpCrashMessage(exception);
            var mainForm = this.MainForm;

            if (mainForm != null)
            {
                ControlUtils.InvokeFNeeded(this.MainForm, () =>
                {
                    using (var form = new CrashReportForm(file, exception))
                    {
                        form.ShowDialog();
                    }

                });

            }
            else
            {
                using (var form = new CrashReportForm(file, exception))
                {
                    form.ShowDialog();
                }

            }

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
            var scheduler = this.Scheduler;
            var tasks = scheduler.GetQueueCopy();

            string text;

            if (scheduler.Busy == true || tasks.Count > 0)
            {
                text = SR.Get("QuitDialog.Text.Busy");
            }
            else
            {
                text = SR.Get("QuitDialog.Text.Common");
            }

            if (this.Config.Values.Program.UserInterfaceRules.ConfirmBeforeExitProgram == true)
            {
                var result = MessageBox.Show(text, SR.Get("QuitDialog.Title"), MessageBoxButtons.OKCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);

                if (result == DialogResult.Cancel)
                {
                    return;
                }

            }

            this.DisposeQuietly();
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

        protected virtual void Dispose(bool disposing)
        {
            this.MainForm.DisposeQuietly();
            this.Scheduler.DisposeQuietly();
            this.NotifyIconManager.DisposeQuietly();
        }

    }

}
