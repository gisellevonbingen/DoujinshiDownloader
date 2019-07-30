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

        public event EventHandler<TaskProgressingEventArgs> Progressed = null;

        private readonly object OperationLock = new object();

        private Thread Thread = null;
        private readonly List<CancellationTokenSource> CancelSources = null;

        private TaskState _State = TaskState.NotStarted;
        public TaskState State { get { lock (this.StateLock) { return this._State; } } }
        public event EventHandler StateChanged = null;
        private readonly object StateLock = new object();

        public Exception Exception { get; private set; }
        public FileArchive DownloadFile { get; private set; }
        public ImageViews ImageViews { get; private set; }

        public int Count { get; private set; } = 0;
        public int Index { get; private set; } = 0;
        public GalleryAgent Agent { get; private set; } = null;

        public DownloadGalleryParameter GalleryParameter { get; private set; } = null;

        private int NextIndex = 0;

        private readonly object IndexLock = null;

        public DownloadTask(DownloadRequest request)
        {
            this.Request = request;
            this.IndexLock = new object();
            this.CancelSources = new List<CancellationTokenSource>();
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
            this.DisposeCancelSources(false);
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
                this.DisposeCancelSources(true);

                ThreadUtils.Join(this.Thread);

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

                this.ThrowIfCanceling();
                this.UpdateState(TaskState.Completed);
            }
            catch (TaskCancelingException)
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

            var downloadPath = PathUtils.GetPath(downloadDirectory, PathUtils.FilterInvalids(request.Title));

            if (downloadToArchive == true)
            {
                this.DownloadFile = new FileArchiveZip(downloadPath + ".zip");
            }
            else
            {
                this.DownloadFile = new FileArchiveDirectory(downloadPath);
            }

            var imageViewUrls = agent.GetGalleryImageViewURLs(galleryURL);
            var imageViews = this.ImageViews = new ImageViews(imageViewUrls);
            this.Count = imageViews.Count;
            this.Index = 0;
            this.Agent = agent;
            this.GalleryParameter = agent.CreateGalleryParameter(galleryURL);
        }

        private void Download()
        {
            var dd = DoujinshiDownloader.Instance;
            var config = dd.Config.Values;

            var threadCount = config.Network.ThreadCount;
            var threads = new List<Thread>();

            for (int i = 0; i < threadCount; i++)
            {
                var thread = new Thread(this.DownloadThreading);
                threads.Add(thread);

                thread.Start();
            }

            foreach (var thread in threads)
            {
                ThreadUtils.Join(thread);
            }

        }

        private void DisposeCancelSources(bool cancel)
        {
            var sources = this.CancelSources;

            lock (sources)
            {
                foreach (var source in sources)
                {
                    if (cancel == true)
                    {
                        try
                        {
                            source.Cancel();
                        }
                        catch (Exception)
                        {

                        }

                    }

                    ObjectUtils.DisposeQuietly(source);
                }

                sources.Clear();
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

                var imageView = this.ImageViews[index];

                try
                {
                    imageView.State = ViewState.Downloading;
                    this.OnProgressing(new TaskProgressingEventArgs(index, imageView));
                    imageView.State = this.Download(imageView);
                }
                catch (TaskCancelingException)
                {
                    return;
                }
                catch (Exception e)
                {
                    imageView.State = ViewState.Exception;
                    Console.WriteLine(e);
                }

                lock (this.IndexLock)
                {
                    this.Index++;
                }

                this.OnProgressing(new TaskProgressingEventArgs(index, imageView));
            }

        }

        protected virtual void OnProgressing(TaskProgressingEventArgs e)
        {
            this.Progressed?.Invoke(this, e);
        }

        protected virtual void ThrowIfCanceling()
        {
            var state = this.State;

            if (state.HasFlag(TaskState.Canceling) == true)
            {
                throw new TaskCancelingException();
            }

        }

        private ViewState Download(ImageView view)
        {
            var agent = this.Agent;
            var downloadRequest = agent.GetGalleryImageDownloadRequest(view.Url, this.GalleryParameter);

            if (downloadRequest == null)
            {
                return ViewState.RequestNotCreate;
            }
            else
            {
                var dd = DoujinshiDownloader.Instance;
                var config = dd.Config.Values;
                var retryCount = config.Network.RetryCount;
                var fileName = this.GetFileName(downloadRequest.URL);

                for (int k = 0; k < retryCount; k++)
                {
                    this.ThrowIfCanceling();

                    try
                    {
                        using (var localStream = new MemoryStream())
                        {
                            var result = this.Download(agent, downloadRequest, localStream);

                            if (result == ViewState.Success)
                            {
                                lock (this.DownloadFile)
                                {
                                    localStream.Position = 0L;
                                    this.DownloadFile.Write(fileName, localStream);
                                }

                                return result;
                            }

                        }

                    }
                    catch (TaskCancelingException)
                    {
                        throw;
                    }
                    catch (Exception e)
                    {
                        if (k + 1 == retryCount)
                        {
                            throw new Exception(downloadRequest.URL, e);
                        }

                    }

                }

            }

            return ViewState.Exception;
        }

        private ViewState Download(GalleryAgent agent, RequestParameter downloadRequest, Stream stream)
        {
            using (var source = new CancellationTokenSource())
            {
                var cancelSources = this.CancelSources;

                try
                {
                    lock (cancelSources)
                    {
                        this.ThrowIfCanceling();
                        cancelSources.Add(source);
                    }

                    using (var response = agent.Explorer.Request(downloadRequest, source.Token))
                    {
                        using (var responseStream = response.ReadToStream())
                        {
                            responseStream.CopyTo(stream);

                            return ViewState.Success;
                        }

                    }

                }
                finally
                {
                    lock (cancelSources)
                    {
                        cancelSources.Remove(source);
                    }

                }

            }

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

    }

}
