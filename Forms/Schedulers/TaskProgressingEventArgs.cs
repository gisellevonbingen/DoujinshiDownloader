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
        public ImageView View { get; }

        public TaskProgressingEventArgs(int index, ImageView view)
        {
            this.Index = index;
            this.View = view;
        }

    }

}
