using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.DoujinshiDownloader.Schedulers;

namespace Giselle.DoujinshiDownloader.Schedulers
{
    public class DownloadProgress
    {
        private readonly DownloadTask Task = null;
        private readonly Dictionary<int, DownloadResult> Map = null;

        public DownloadProgress(DownloadTask task)
        {
            if (task == null)
            {
                throw new ArgumentNullException(nameof(task));
            }

            this.Task = task;
            task.Progressed += this.OnTaskProgressed;

            this.Map = new Dictionary<int, DownloadResult>();
        }

        private void OnTaskProgressed(object sender, TaskProgressingEventArgs e)
        {
            var index = e.Index;

            if (this.CheckIndexInRange(index) == true)
            {
                var map = this.Map;

                lock (map)
                {
                    map[index] = e.Result;
                }

            }

        }

        protected bool CheckIndexInRange(int index)
        {
            return 0 <= index && index < this.Task.Count;
        }

        protected void RequireIndexInRange(int index)
        {
            if (this.CheckIndexInRange(index) == false)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

        }

        public DownloadResult this[int index]
        {
            get
            {
                this.RequireIndexInRange(index);

                var result = DownloadResult.Exception;
                var map = this.Map;

                lock (map)
                {
                    return map.TryGetValue(index, out result) ? result : DownloadResult.StandBy;
                }

            }

        }

        public int Count(DownloadResult flag)
        {
            var allCount = this.Task.Count;
            var count = 0;

            for (int i = 0; i < allCount; i++)
            {
                if (this[i].HasFlag(flag) == true)
                {
                    count++;
                }

            }

            return count;
        }

    }

}
