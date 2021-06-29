using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Giselle.Commons;
using Giselle.DoujinshiDownloader.Doujinshi;
using Giselle.DoujinshiDownloader.Utils;
using Giselle.Forms;

namespace Giselle.DoujinshiDownloader
{
    public class HitomiOutdateScheduler : IDisposable
    {
        public object StateLock { get; } = new object();
        public Thread Thread { get; private set; } = null;
        public bool LastResult { get; private set; } = true;

        public HitomiOutdateScheduler()
        {

        }

        private void Run()
        {
            try
            {
                while (true)
                {
                    var agent = DownloadMethod.Hitomi.CreateAgent() as HitomiAgent;

                    var md5 = agent.GetLtnCommon().GetMD5String();
                    var result = HitomiAgent.CompareMD5(md5);

                    if (this.LastResult == true && result == false)
                    {
                        DoujinshiDownloader.Instance.MainForm.InvokeFNeeded(() =>
                        {
                            var title = SR.Get("HitomiOutdateScheduler.Notify.Title");
                            var text = SR.Get("DownloadSelect.Verify.HitomiOutdateError");
                            DoujinshiDownloader.Instance.NotifyIconManager.Show(title, text);
                        });
                    }

                    this.LastResult = result;
                    Thread.Sleep(1000 * 3600);
                }

            }
            catch (Exception)
            {

            }

        }

        public void Start()
        {
            lock (this.StateLock)
            {
                this.Stop();
                this.LastResult = true;

                this.Thread = new Thread(this.Run);
                this.Thread.Start();
            }

        }

        public void Stop()
        {
            lock (this.StateLock)
            {
                ThreadUtils.InterruptAndJoin(this.Thread);
                this.Thread = null;
            }

        }

        protected virtual void Dispose(bool disposing)
        {
            this.Stop();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            this.Dispose(true);
        }

        ~HitomiOutdateScheduler()
        {
            this.Dispose(false);
        }

    }

}
