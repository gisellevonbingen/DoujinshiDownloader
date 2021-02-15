using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.DoujinshiDownloader.Doujinshi;

namespace Giselle.DoujinshiDownloader.Schedulers
{
    public class TaskImageDownloadEventArgs : EventArgs
    {
        public DownloadTask Task { get; }
        public ImageViewState View { get; }
        public GalleryImagePath Image { get; }
        public byte[] Data { get; }
        public int Index { get; }
        public int RetryCount { get; }

        public TaskImageDownloadEventArgs(DownloadTask task, ImageViewState view, GalleryImagePath image, byte[] data, int index, int retryCount)
        {
            this.Task = task;
            this.View = view;
            this.Image = image;
            this.Data = data;
            this.Index = index;
            this.RetryCount = retryCount;
        }

    }

}
