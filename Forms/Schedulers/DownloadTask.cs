using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Giselle.Commons;
using Giselle.Commons.Net;
using Giselle.DoujinshiDownloader.Doujinshi;
using Giselle.DoujinshiDownloader.Utils;

namespace Giselle.DoujinshiDownloader.Schedulers
{
    public class DownloadTask : IDisposable
    {
        public DownloadRequest Request { get; }

        public event EventHandler<TaskProgressingEventArgs> Progressed = null;
        public event EventHandler<TaskProgressingEventArgs> Downloading = null;

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
            var site = request.Validation.Method.Site;
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
                    this.OnProgressed(new TaskProgressingEventArgs(imageViewState));

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

                this.OnProgressed(new TaskProgressingEventArgs(imageViewState));
            }

        }

        protected virtual void OnProgressed(TaskProgressingEventArgs e)
        {
            this.Progressed?.Invoke(this, e);
        }

        protected virtual void OnDownloading(TaskProgressingEventArgs e)
        {
            this.Downloading?.Invoke(this, e);
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
            var site = this.Request.Validation.Method.Site;
            var input = this.Request.Validation.Input;
            var imageView = viewState.View;
            var imagePath = agent.GetGalleryImagePath(site, input, imageView, values);

            if (imagePath.ImageUrl == null)
            {
                return "RequestNotCreate";
            }
            else
            {
                var fileName = imageView.FileName ?? new Uri(imagePath.ImageUrl).GetFileName();

                for (int k = 0; k < agent.RetryCount + 1; k++)
                {
                    try
                    {
                        this.ThrowIfCancelRequested();

                        var bytes = this.Download(agent, site, input, viewState, imagePath, values);
                        agent.Validate(site, input, imageView, imagePath, values, bytes);
                        this.OnImageDownload(new TaskImageDownloadEventArgs(this, viewState, imagePath, bytes, index, k));

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
                            if (string.IsNullOrWhiteSpace(imagePath.ReloadUrl) == false)
                            {
                                imagePath = agent.ReloadImagePath(site, input, imageView, imagePath, values);
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

        private byte[] Download(GalleryAgent agent, Site site, DownloadInput input, ImageViewState imageViewState, GalleryImagePath imagePath, GalleryParameterValues values)
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

                    var downloadRequest = agent.CreateImageRequest(site, input, imageViewState.View, imagePath, values);

                    using (var response = agent.Explorer.Request(downloadRequest, source))
                    {
                        imageViewState.Length = response.ContentLength;
                        imageViewState.Position = 0L;

                        using (var responseStream = response.ReadAsStream())
                        {
                            return this.Download(imageViewState, responseStream);
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

        private byte[] Download(ImageViewState imageViewState, Stream responseStream)
        {
            using (var ms = new MemoryStream())
            {
                var buffer = new byte[81920];
                var len = 0;

                for (; (len = responseStream.Read(buffer, 0, buffer.Length)) > 0;)
                {
                    ms.Write(buffer, 0, len);
                    imageViewState.Position += len;

                    try
                    {
                        this.OnDownloading(new TaskProgressingEventArgs(imageViewState));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }

                }

                return ms.ToArray();
            }

        }

    }

}
