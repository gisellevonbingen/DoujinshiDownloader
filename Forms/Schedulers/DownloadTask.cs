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
    public class DownloadTask : IDisposable
    {
        public DownloadRequest Request { get; }
        public DownloadProgress Progress { get; }

        public event EventHandler<TaskProgressingEventArgs> Progressed = null;

        private readonly object OperationLock = new object();

        private Thread Thread = null;
        private readonly List<Thread> DownloadThreads = null;

        private TaskState _State = TaskState.NotStarted;
        public TaskState State { get { lock (this.StateLock) { return this._State; } } }
        public event EventHandler StateChanged = null;
        private readonly object StateLock = new object();

        public Exception Exception { get; private set; }
        public FileArchive DownloadFile { get; private set; }

        private List<string> ViewURLs = null;

        public int Count { get; private set; } = 0;
        public int Index { get; private set; } = 0;
        private GalleryAgent _Agent = null;
        public GalleryAgent Agent { get { return this._Agent; } }

        public DownloadGalleryParameter GalleryParameter { get; private set; } = null;

        private int NextIndex = 0;

        private readonly object IndexLock = null;

        public DownloadTask(DownloadRequest request)
        {
            this.Request = request;
            this.Progress = new DownloadProgress(this);
            this.IndexLock = new object();
            this.DownloadThreads = new List<Thread>();
        }

        ~DownloadTask()
        {
            this.Dispose(false);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            this.Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            ObjectUtils.DisposeQuietly(this.DownloadFile);
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
            finally
            {
                this.Dispose();
            }

        }

        private void Prepare()
        {
            var dd = DoujinshiDownloader.Instance;
            var config = dd.Config.Values;

            var request = this.Request;
            var method = request.DownloadMethod;
            var galleryURL = method.Site.ToURL(request.DownloadInput);
            var agent = method.CreateAgent();

            var downloadToArchive = config.Content.DownloadToArchive;
            var downloadDirectory = config.Content.DownloadDirectory;
            Directory.CreateDirectory(downloadDirectory);

            var ffff = PathUtils.GetPath(downloadDirectory, PathUtils.FilterInvalids(request.Title));

            if (downloadToArchive == true)
            {
                this.DownloadFile = new FileArchiveZip(ffff + ".zip");
            }
            else
            {
                this.DownloadFile = new FileArchiveDirectory(ffff);
            }

            var viewURLs = this.ViewURLs = agent.GetGalleryImageViewURLs(galleryURL);
            this.Count = viewURLs.Count;
            this.Index = 0;
            this._Agent = agent;
            this.GalleryParameter = agent.CreateGalleryParameter(galleryURL);
        }

        private void Download()
        {
            var dd = DoujinshiDownloader.Instance;
            var config = dd.Config.Values;

            var threadCount = config.Network.ThreadCount;
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
                    this.OnProgressing(new TaskProgressingEventArgs(index, url, DownloadResult.Downloading));
                    result = this.Download(url);
                }
                catch (ThreadAbortException)
                {
                    return;
                }
                catch (Exception e)
                {
                    result = DownloadResult.Exception;
                    Console.WriteLine(e);
                }

                lock (this.IndexLock)
                {
                    this.Index++;
                }

                this.OnProgressing(new TaskProgressingEventArgs(index, url, result));
            }

        }

        protected virtual void OnProgressing(TaskProgressingEventArgs e)
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
                var config = dd.Config.Values;
                var retryCount = config.Network.RetryCount;
                var fileName = this.GetFileName(downloadRequest.URL);
                var buffer = new byte[16 * 1024];

                for (int k = 0; k < retryCount; k++)
                {
                    try
                    {
                        using (var localStream = new MemoryStream())
                        {
                            var result = this.Download(agent, downloadRequest, localStream, buffer);

                            if (result == DownloadResult.Success)
                            {
                                lock (this.DownloadFile)
                                {
                                    localStream.Position = 0L;
                                    this.DownloadFile.Write(fileName, localStream);
                                }

                            }

                            return result;
                        }

                    }
                    catch (ThreadAbortException)
                    {
                        return DownloadResult.Exception;
                    }
                    catch (Exception e)
                    {
                        if (k + 1 == retryCount)
                        {
                            throw new Exception(pagePath, e);
                        }

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
