using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Giselle.Commons;
using Giselle.DoujinshiDownloader.Doujinshi;
using Giselle.DoujinshiDownloader.Forms;
using Giselle.DoujinshiDownloader.Resources;
using Giselle.DoujinshiDownloader.Schedulers;
using Giselle.DoujinshiDownloader.Utils;

namespace Giselle.DoujinshiDownloader
{
    public class DoujinshiDownloader : IDisposable
    {
        private static DoujinshiDownloader _Instance = null;
        public static DoujinshiDownloader Instance { get { return _Instance; } }

        [STAThread]
        public static void Main(string[] args)
        {
            var instance = _Instance = new DoujinshiDownloader();
            instance.Run();
        }

        private Settings _Settings = null;
        public Settings Settings { get { return this._Settings; } }

        private FontManager _FontManager = null;
        public FontManager FontManager { get { return this._FontManager; } }

        private DownloadScheduler _Scheduler = null;
        public DownloadScheduler Scheduler { get { return this._Scheduler; } }

        private MainForm _MainForm = null;
        public MainForm MainForm { get { return this._MainForm; } }

        public DoujinshiDownloader()
        {
            this._Settings = new Settings(PathUtils.GetPath("Configuration.json"));
            this._FontManager = new FontManager();
            this._Scheduler = new DownloadScheduler();
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

            ObjectUtils.DisposeQuietly(this.FontManager);

        }

        private void Run()
        {
            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                this.Settings.Load();

                this.Scheduler.Start();

                var form = this._MainForm = new MainForm();
                Application.Run(form);
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

            using (var form = new CrashReportForm(file, exception))
            {
                form.ShowDialog();
            }

        }

        public string GetCrashReportsDirectory()
        {
            var directory = PathUtils.GetPath("CrashReports");
            Directory.CreateDirectory(directory);

            return directory;
        }

        private string DumpCrashMessage(Exception exception)
        {
            try
            {
                var directory = this.GetCrashReportsDirectory();

                var dateTime = DateTime.Now;
                var file = PathUtils.GetPath(directory, dateTime.ToString("yyyy_MM_dd HH_mm_ss_fff") + ".log");

                File.WriteAllText(file, string.Concat(exception));

                return file;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }

        }

    }

}
