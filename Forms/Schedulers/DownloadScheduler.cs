using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Giselle.Commons;

namespace Giselle.DoujinshiDownloader.Schedulers
{
    public class DownloadScheduler : IDisposable
    {
        private readonly object StateLockObject = new object();

        private bool Stopping = false;

        public bool Busy { get; private set; } = false;

        private ManualResetEventSlim ResetEvent = null;
        private Thread ExecuteThread = null;

        private readonly List<DownloadTask> Queue = null;

        public DownloadScheduler()
        {
            this.Queue = new List<DownloadTask>();
        }

        ~DownloadScheduler()
        {
            this.Dispose(false);
        }

        private void Execute(DownloadTask task)
        {
            try
            {
                if (task.State.HasFlag(TaskState.NotStarted) == true)
                {
                    task.Start();
                    task.WaitForComplete();
                }

            }
            catch (ThreadAbortException)
            {
                task.Cancel();
                throw;
            }
            catch (Exception e)
            {
                DoujinshiDownloader.Instance.ShowCrashMessageBox(e);
            }

        }

        private void ExecuteThreading()
        {
            try
            {
                while (true)
                {
                    this.ResetEvent.Wait();

                    try
                    {
                        this.Busy = true;

                        DownloadTask[] array = null;

                        lock (this.Queue)
                        {
                            array = this.Queue.ToArray();
                            this.Queue.Clear();

                            this.ResetEvent.Reset();
                        }

                        foreach (var task in array)
                        {
                            try
                            {
                                this.Execute(task);
                            }
                            finally
                            {
                                task.DisposeQuietly();
                            }

                        }

                    }
                    finally
                    {
                        this.Busy = false;
                    }

                }

            }
            catch (ThreadAbortException)
            {

            }
            catch (Exception e)
            {
                DoujinshiDownloader.Instance.ShowCrashMessageBox(e);
            }

        }

        public void Start()
        {
            lock (this.StateLockObject)
            {
                this.Stop();

                this.ResetEvent = new ManualResetEventSlim(false);

                this.ExecuteThread = new Thread(this.ExecuteThreading);
                this.ExecuteThread.Start();
            }

        }

        public void Stop()
        {
            lock (this.StateLockObject)
            {
                if (this.Stopping == true)
                {
                    return;
                }

                try
                {
                    this.Stopping = true;

                    ThreadUtils.AbortAndJoin(this.ExecuteThread);
                    this.ExecuteThread = null;

                    this.ResetEvent.DisposeQuietly();
                    this.ResetEvent = null;
                }
                finally
                {
                    this.Stopping = false;
                }
            }

        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            this.Stop();
        }

        public List<DownloadTask> GetQueueCopy()
        {
            var queue = this.Queue;

            lock (queue)
            {
                return new List<DownloadTask>(queue);
            }

        }

        public DownloadTask AddQueue(DownloadRequest request)
        {
            var queue = this.Queue;
            DownloadTask task = null;

            lock (queue)
            {
                task = new DownloadTask(request);
                queue.Add(task);

                this.ResetEvent.Set();
            }

            return task;
        }

        public void CancelAll()
        {
            var tasks = this.Queue;
            DownloadTask[] runningTasks = null;

            lock (tasks)
            {
                runningTasks = tasks.ToArray();
            }

            foreach (var task in runningTasks)
            {
                task.Cancel();
            }

        }

    }

}
