using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Giselle.Commons;
using Giselle.Commons.Web;
using Giselle.DoujinshiDownloader.Doujinshi;
using Giselle.DoujinshiDownloader.Utils;

namespace Giselle.DoujinshiDownloader.Schedulers
{
    public class DownloadTask : IDisposable
    {
        public DownloadRequest Request { get; }

        public event EventHandler<TaskProgressingEventArgs> Progressed = null;

        private readonly object OperationLock = new object();

        private Thread Thread = null;
        private bool Disposing = false;
        private readonly List<CancellationTokenSource> CancelSources = null;

        public TaskState State { get; private set; } = TaskState.NotStarted;
        public event EventHandler StateChanged = null;
        private readonly object StateLock = new object();

        public Exception Exception { get; private set; }
        public FileArchive DownloadFile { get; private set; }
        public ImageViewStates ImageViewStates { get; private set; }

        public int Count { get; private set; } = 0;
        public int Index { get; private set; } = 0;
        public GalleryAgent Agent { get; private set; } = null;


        private int NextIndex = 0;

        private readonly object IndexLock = null;

        public event EventHandler<TaskImageDownloadEventArgs> ImageDownload;

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
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this.Disposing == true)
            {
                return;
            }

            try
            {
                this.Disposing = true;
                this.Cancel();
                this.DownloadFile.DisposeQuietly();
                this.DisposeCancelSources(false);
            }
            finally
            {
                this.Disposing = false;
            }

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
                this.State = state;
            }

            this.OnStateChanged(EventArgs.Empty);
        }

        protected virtual void OnStateChanged(EventArgs e)
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

        public bool CancelRequested
        {
            get
            {
                var state = this.State;
                return state.HasFlag(TaskState.Canceling) == true || state.HasFlag(TaskState.Cancelled) == true;
            }

        }

        public bool Running => this.Thread != null;

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
            if (this.CancelRequested == true)
            {
                return;
            }

            lock (this.OperationLock)
            {
                this.UpdateState(TaskState.Canceling);
                this.DisposeCancelSources(true);

                ThreadUtils.Join(this.Thread);
                this.Thread = null;

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

                this.ThrowIfCancelRequested();
                this.UpdateState(TaskState.Completed);
            }
            catch (TaskCancelingException)
            {

            }
            catch (Exception e)
            {
                this.Exception = e;
                this.UpdateState(TaskState.Completed | TaskState.Excepted);

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
            var agent = request.Validation.Agent;
            var site = request.Validation.Site;
            var input = request.Validation.Input;
            var galleryValues = request.GalleryParameterValues;

            var downloadToArchive = config.Content.DownloadToArchive;
            var downloadDirectory = config.Content.DownloadDirectory;
            Directory.CreateDirectory(downloadDirectory);

            var downloadPath = PathUtils.GetPath(downloadDirectory, PathUtils.FilterInvalids(request.FileName));

            if (downloadToArchive == true)
            {
                this.DownloadFile = new FileArchiveZip(downloadPath + ".zip");
            }
            else
            {
                this.DownloadFile = new FileArchiveDirectory(downloadPath);
            }

            var galleryImageViews = agent.GetGalleryImageViews(site, input, galleryValues);
            var imageViewStates = this.ImageViewStates = new ImageViewStates(galleryImageViews);
            this.Count = imageViewStates.Count;
            this.Index = 0;
            this.Agent = agent;
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
                        source.ExecuteQuietly(t => t.Cancel());
                    }

                    source.DisposeQuietly();
                }

                sources.Clear();
            }

        }

        private void DownloadThreading()
        {
            var request = this.Request;
            var values = request.GalleryParameterValues;

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

                var imageViewState = this.ImageViewStates[index];

                try
                {
                    imageViewState.State = ViewState.Downloading;
                    this.OnProgressing(new TaskProgressingEventArgs(index, imageViewState));

                    var exceptionMessage = this.Download(index, imageViewState, values);
                    imageViewState.State = string.IsNullOrWhiteSpace(exceptionMessage) ? ViewState.Success : ViewState.Exception;
                    imageViewState.ExceptionMessage = exceptionMessage;
                }
                catch (TaskCancelingException)
                {
                    return;
                }
                catch (Exception e)
                {
                    if (this.CancelRequested == true)
                    {
                        return;
                    }

                    Console.WriteLine(e);

                    imageViewState.State = ViewState.Exception;
                    imageViewState.ExceptionMessage = e.GetType().Name + " : " + e.Message;
                }

                lock (this.IndexLock)
                {
                    this.Index++;
                }

                this.OnProgressing(new TaskProgressingEventArgs(index, imageViewState));
            }

        }

        protected virtual void OnProgressing(TaskProgressingEventArgs e)
        {
            this.Progressed?.Invoke(this, e);
        }

        protected virtual void ThrowIfCancelRequested()
        {
            if (this.CancelRequested == true)
            {
                throw new TaskCancelingException();
            }

        }

        private string Download(int index, ImageViewState viewState, GalleryParameterValues values)
        {
            var agent = this.Agent;
            var site = this.Request.Validation.Site;
            var input = this.Request.Validation.Input;

            var image = agent.GetGalleryImage(site, input, viewState.View.Url, values);

            if (image.ImageUrl == null)
            {
                return "RequestNotCreate";
            }
            else
            {
                var fileName = viewState.View.FileName ?? new Uri(image.ImageUrl).GetFileName();

                for (int k = 0; k < agent.RetryCount + 1; k++)
                {
                    try
                    {
                        this.ThrowIfCancelRequested();

                        var downloadRequest = agent.CreateImageRequest(site, input, image.ImageUrl, values);
                        var bytes = this.Download(agent, downloadRequest);
                        this.OnImageDownload(new TaskImageDownloadEventArgs(this, viewState, image, bytes, index, k));

                        lock (this.DownloadFile)
                        {
                            this.DownloadFile.Write(fileName, bytes);
                        }

                        return null;
                    }
                    catch (ImageRequestCreateException e)
                    {
                        return e.Message;
                    }
                    catch (TaskCancelingException)
                    {
                        throw;
                    }
                    catch (Exception e)
                    {
                        if (this.CancelRequested == true)
                        {
                            throw;
                        }

                        Console.WriteLine(e);

                        if (k + 1 == agent.RetryCount)
                        {
                            return "Network";
                        }
                        else
                        {
                            if (string.IsNullOrWhiteSpace(image.ReloadUrl) == false)
                            {
                                image = agent.ReloadImage(site, input, image.ImageUrl, image.ReloadUrl, values);
                            }

                        }

                    }

                }

            }

            return "Unknown";
        }

        protected virtual void OnImageDownload(TaskImageDownloadEventArgs e)
        {
            this.ImageDownload?.Invoke(this, e);
        }

        private byte[] Download(GalleryAgent agent, WebRequestParameter downloadRequest)
        {
            using (var source = new CancellationTokenSource())
            {
                var cancelSources = this.CancelSources;

                try
                {
                    lock (cancelSources)
                    {
                        this.ThrowIfCancelRequested();
                        cancelSources.Add(source);
                    }

                    using (var response = agent.Explorer.Request(downloadRequest, source))
                    {
                        using (var responseStream = response.ReadAsStream())
                        {
                            using (var ms = new MemoryStream())
                            {
                                responseStream.CopyTo(ms);
                                return ms.ToArray();
                            }

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

    }

}
