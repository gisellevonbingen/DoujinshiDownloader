﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Giselle.Commons;
using Giselle.Commons.Net;
using Giselle.Commons.Threading;
using Giselle.DoujinshiDownloader.Configs;
using Giselle.DoujinshiDownloader.Doujinshi;
using Giselle.DoujinshiDownloader.Utils;
using ImageMagick;

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
        public string DownloadDirectory { get; private set; }
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

                if (this.Cancellable == true)
                {
                    this.Cancel();
                }
                else
                {
                    this.Thread.JoinNotCurrent();
                    this.Thread = null;
                }

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

        public bool Cancellable
        {
            get
            {
                var state = this.State;
                return state.HasFlag(TaskState.Canceling) == false && state.HasFlag(TaskState.Completed) == false;
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
                if (this.CancelRequested == true)
                {
                    return;
                }

                this.UpdateState(TaskState.Canceling);
                this.DisposeCancelSources(true);

                this.Thread.JoinNotCurrent();
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

                try
                {
                    this.Download();
                }
                finally
                {
                    this.DownloadFile.DisposeQuietly();
                }

                this.UpdateState(TaskState.Completed);
            }
            catch (Exception e)
            {
                this.Exception = e;
                this.UpdateState(TaskState.Completed | TaskState.Excepted);

                DoujinshiDownloader.Instance.ShowCrashMessageBox(e);
            }

        }

        private void Prepare()
        {
            var dd = DoujinshiDownloader.Instance;
            var config = dd.Config.Values;

            var request = this.Request;
            var agent = request.Validation.Agent;

            var downloadToArchive = config.Content.DownloadToArchive;
            var downloadDirectory = config.Content.DownloadDirectory;
            this.DownloadDirectory = downloadDirectory;
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

            var galleryImageViews = agent.GetGalleryImageViews();
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
                thread.JoinNotCurrent();
            }

            this.DownloadFile.DisposeQuietly();
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

                    var imageViewResult = this.Download(index, imageViewState);
                    imageViewState.State = imageViewResult == null ? ViewState.Success : ViewState.Exception;
                    imageViewState.Error = imageViewResult;
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
                    imageViewState.Error = new ImageViewError() { Message = "Unknown", Exception = e };
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

        private ImageViewError Download(int index, ImageViewState viewState)
        {
            var agent = this.Agent;
            var imageView = viewState.View;
            var imagePath = agent.GetGalleryImagePath(imageView);

            if (imagePath.ImageUrl == null)
            {
                return new ImageViewError() { Message = "RequestNotCreate" };
            }
            else
            {
                var config = DoujinshiDownloader.Instance.Config.Values.Network;
                var retryCount = config.RetryCount;
                var fileName = imageView.FileName ?? new Uri(imagePath.ImageUrl).GetFileName();

                for (var k = 0; k < retryCount + 1; k++)
                {
                    try
                    {
                        this.ThrowIfCancelRequested();

                        var bytes = this.Download(agent, viewState, imagePath);
                        agent.Validate(imageView, imagePath, bytes);
                        this.OnImageDownload(new TaskImageDownloadEventArgs(this, viewState, imagePath, bytes, index, k));

                        var pair = this.ConvertForWrite(fileName, bytes);
                        fileName = pair.Item1;
                        bytes = pair.Item2;

                        lock (this.DownloadFile)
                        {
                            this.DownloadFile.Write(fileName, bytes);
                        }

                        return null;
                    }
                    catch (ImageRequestCreateException e)
                    {
                        return new ImageViewError() { Message = "Unknown", Exception = e };
                    }
                    catch (HttpStatusCodeException e)
                    {
                        if (e.Code == HttpStatusCode.ServiceUnavailable)
                        {
                            Thread.Sleep(config.ServiceUnavailableSleep);
                        }
                        else
                        {
                            return new ImageViewError() { Message = "Unknown", Exception = e };
                        }

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

                        if (k + 1 == retryCount)
                        {
                            return new ImageViewError() { Message = "Network", Exception = e };
                        }
                        else
                        {
                            if (string.IsNullOrWhiteSpace(imagePath.ReloadUrl) == false)
                            {
                                imagePath = agent.ReloadImagePath(imageView, imagePath);
                            }

                        }

                    }

                }

            }

            return new ImageViewError() { Message = "Unknown" };
        }

        private (string, byte[]) ConvertForWrite(string fileName, byte[] bytes)
        {
            var config = DoujinshiDownloader.Instance.Config.Values.Content;
            var multiFormat = config.MultiFrameConvertType;
            var singleFormat = config.SingleFrameConvertType;

            if (multiFormat == ImageConvertType.Original && singleFormat == ImageConvertType.Original)
            {
                return (fileName, bytes);
            }

            try
            {
                var r1 = MagickFormatInfo.Create(bytes).Format;

                using (var image = new MagickImage(bytes))
                {
                    var format = MagickFormat.Unknown;

                    if (image.AnimationDelay > 0)
                    {
                        format = multiFormat.Format;
                    }
                    else
                    {
                        format = singleFormat.Format;
                    }

                    if (format != MagickFormat.Unknown && format != image.Format)
                    {
                        using (var ms = new MemoryStream())
                        {
                            using (var collection = new MagickImageCollection(bytes))
                            {
                                collection.Write(ms, format);
                            }

                            return (Path.ChangeExtension(fileName, format.ToString().ToLowerInvariant()), ms.ToArray());
                        }

                    }

                }

            }
            catch
            {

            }

            return (fileName, bytes);
        }

        protected virtual void OnImageDownload(TaskImageDownloadEventArgs e)
        {
            this.ImageDownload?.Invoke(this, e);
        }

        private byte[] Download(GalleryAgent agent, ImageViewState imageViewState, GalleryImagePath imagePath)
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

                    var downloadRequest = agent.CreateImageRequest(imageViewState.View, imagePath);

                    using (var response = agent.Explorer.Request(downloadRequest, source))
                    {
                        if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
                        {
                            throw new HttpStatusCodeException(response.StatusCode);
                        }

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
