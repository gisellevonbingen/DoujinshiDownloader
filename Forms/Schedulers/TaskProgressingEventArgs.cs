using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.DoujinshiDownloader.Schedulers
{
    public class TaskProgressingEventArgs : EventArgs
    {
        public int Index { get; }
        public ImageViewState View { get; }

        public TaskProgressingEventArgs(int index, ImageViewState view)
        {
            this.Index = index;
            this.View = view;
        }

    }

}
