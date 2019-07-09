using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Giselle.Commons;
using Giselle.DoujinshiDownloader.Doujinshi;
using Giselle.DoujinshiDownloader.Utils;
using Giselle.DoujinshiDownloader.Web;

namespace Giselle.DoujinshiDownloader.Schedulers
{
    public class DownloadTask
    {
        public DownloadRequest Request { get; }
        public DownloadProgress Progress { get; }

        public event DownloadTaskProgressingEventHandler Progressed = null;

        private readonly object OperationLock = new object();

        private Thread Thread = null;
        private readonly List<Thread> DownloadThreads = null;

        private TaskState _State = TaskState.NotStarted;
        public TaskState State { get { lock (this.StateLock) { return this._State; } } }
        public event EventHandler StateChanged = null;
        private readonly object StateLock = new object();

        public Exception Exception { get; private set; }
        public string DownloadDirectory { get; private set; }

        private List<string> ViewURLs = null;

        private int _Count = 0;
        public int Count { get { return this._Count; } }
        private int _Index = 0;
        public int Index { get { return this._Index; } }
        private GalleryAgent _Agent = null;
        public GalleryAgent Agent { get { return this._Agent; } }
        private DownloadGalleryParameter _GalleryParameter = null;
        public DownloadGalleryParameter GalleryParameter { get { return this._GalleryParameter; } }

        private int NextIndex = 0;

        private readonly object IndexLock = null;

        public DownloadTask(DownloadRequest request)
        {
            this.Request = request;
            this.Progress = new DownloadProgress(this);
            this.IndexLock = new object();
            this.DownloadThreads = new List<Thread>();
        }

        private void RequireState(TaskState require)
        {
            lock (this.StateLock)
            {
                var state = this.State;

                if (state != require)
                {
                    throw new TaskInvalidStateException($"Task state is already [{state}]");
                }

            }

        }

        private void UpdateState(TaskState state)
        {
            lock (this.StateLock)
            {
                this._State = state;
            }

            this.OnStateChanged(EventArgs.Empty);
        }

        private void OnStateChanged(EventArgs e)
        {
            this.StateChanged?.Invoke(this, e);
        }

        public void Start()
        {
            lock (this.OperationLock)
            {
                this.RequireState(TaskState.NotStarted);

                this.UpdateState(TaskState.Starting);
                var thread = this.Thread = new Thread(this.Run);
                thread.Start();
            }

        }

        public void WaitForComplete()
        {
            Thread thread = null;

            lock (this.OperationLock)
            {
                thread = this.Thread;
            }

            if (thread == null)
            {
                throw new TaskInvalidStateException($"Task state is not started");
            }

            thread.Join();
        }

        public void Cancel()
        {
            lock (this.OperationLock)
            {
                this.UpdateState(TaskState.Canceling);

                ThreadUtils.AbortAndJoin(this.Thread);
                this.Thread = null;

                var downloadThreads = this.DownloadThreads;

                lock (downloadThreads)
                {
                    foreach (var thread in downloadThreads)
                    {
                        ThreadUtils.AbortAndJoin(thread);
                    }

                    downloadThreads.Clear();
                }

                this.UpdateState(TaskState.Completed | TaskState.Cancelled);
            }

        }

        private void Run()
        {
            try
            {
                this.Prepare();

                this.UpdateState(TaskState.Running);

                this.Download();

                this.UpdateState(TaskState.Completed);
            }
            catch (ThreadAbortException)
            {

            }
            catch (Exception e)
            {
                lock (this.StateLock)
                {
                    this.Exception = e;
                    this.UpdateState(TaskState.Completed | TaskState.Excepted);
                }

                DoujinshiDownloader.Instance.ShowCrashMessageBox(e);
            }

        }

        private void Prepare()
        {
            var dd = DoujinshiDownloader.Instance;
            var settings = dd.Settings;

            var request = this.Request;
            var method = request.DownloadMethod;
            var galleryURL = method.Site.ToURL(request.DownloadInput);
            var agent = method.CreateAgent();


            var downloadDirectory = PathUtils.GetPath(settings.DownloadDirectory, PathUtils.FilterInvalids(request.Title));
            Directory.CreateDirectory(downloadDirectory);
            this.DownloadDirectory = downloadDirectory;

            var viewURLs = this.ViewURLs = agent.GetGalleryImageViewURLs(galleryURL);
            this._Count = viewURLs.Count;
            this._Index = 0;
            this._Agent = agent;
            this._GalleryParameter = agent.CreateGalleryParameter(galleryURL);
        }

        private void Download()
        {
            var dd = DoujinshiDownloader.Instance;
            var settings = dd.Settings;

            var threadCount = settings.ThreadCount;
            var threads = this.DownloadThreads;

            lock (threads)
            {
                for (int i = 0; i < threadCount; i++)
                {
                    var thread = new Thread(this.DownloadThreading);
                    threads.Add(thread);
                    thread.Start();
                }

            }

            var downloadThreads = threads.ToArray();

            foreach (var thread in downloadThreads)
            {
                thread.Join();
            }

            lock (threads)
            {
                threads.Clear();
            }

        }

        private void DownloadThreading()
        {
            while (true)
            {
                var index = 0;

                lock (this.IndexLock)
                {
                    index = this.NextIndex;

                    if (index >= this.Count)
                    {
                        return;
                    }

                    this.NextIndex += 1;
                }

                var url = this.ViewURLs[index];
                var result = DownloadResult.Exception;

                try
                {
                    this.OnProgressing(new DownloadTaskProgressingEventArgs(index, url, DownloadResult.Downloading));
                    result = this.Download(url);
                }
                catch (Exception e)
                {
                    result = DownloadResult.Exception;
                    DoujinshiDownloader.Instance.ShowCrashMessageBox(e);
                }

                lock (this.IndexLock)
                {
                    this._Index++;
                }

                this.OnProgressing(new DownloadTaskProgressingEventArgs(index, url, result));
            }

        }

        protected virtual void OnProgressing(DownloadTaskProgressingEventArgs e)
        {
            this.Progressed?.Invoke(this, e);
        }

        private DownloadResult Download(string pagePath)
        {
            var request = this.Request;
            var method = request.DownloadMethod;
            var agent = this.Agent;

            var downloadParameter = method.CreateDownloadParameter();
            var downloadRequest = agent.GetGalleryImageDownloadRequest(pagePath, this.GalleryParameter, downloadParameter);

            if (downloadRequest == null)
            {
                return DownloadResult.RequestNotCreate;
            }
            else
            {
                var dd = DoujinshiDownloader.Instance;
                var settings = dd.Settings;
                var retryCount = settings.RetryCount;
                var fileName = this.GetFileName(downloadRequest.URL);
                var downloadPath = Path.Combine(this.DownloadDirectory, fileName);
                var buffer = new byte[16 * 1024];

                for (int k = 0; k < retryCount; k++)
                {
                    try
                    {
                        using (var localStream = new FileStream(downloadPath, FileMode.Create))
                        {
                            return this.Download(agent, downloadRequest, localStream, buffer);
                        }

                    }
                    catch (Exception)
                    {

                    }

                }

            }

            return DownloadResult.Exception;
        }

        private string GetFileName(string url)
        {
            var fileName = new Uri(url).LocalPath;
            int slashIndex = fileName.LastIndexOf('/');

            if (slashIndex > -1)
            {
                fileName = fileName.Substring(slashIndex + 1);
            }

            return fileName;
        }

        private DownloadResult Download(GalleryAgent agent, RequestParameter downloadRequest, Stream stream, byte[] buffer)
        {
            using (var response = agent.Explorer.Request(downloadRequest))
            {
                using (var responseStream = response.ReadToStream())
                {
                    for (int len = 0; (len = responseStream.Read(buffer, 0, buffer.Length)) > 0;)
                    {
                        stream.Write(buffer, 0, len);
                    }

                    return DownloadResult.Success;
                }

            }

        }

    }

}
