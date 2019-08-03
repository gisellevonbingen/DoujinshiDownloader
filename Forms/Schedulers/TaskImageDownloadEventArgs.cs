using Giselle.DoujinshiDownloader.Doujinshi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.DoujinshiDownloader.Schedulers
{
    public class TaskImageDownloadEventArgs : EventArgs
    {
        public DownloadTask Task { get; }
        public ImageView View { get; }
        public GalleryImage Image { get; }
        public byte[] Data { get; }
        public int Index { get; }
        public int RetryCount { get; }

        public TaskImageDownloadEventArgs(DownloadTask task, ImageView view, GalleryImage image, byte[] data, int index, int retryCount)
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
