using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.DoujinshiDownloader.Schedulers
{
    public class TaskProgressingEventArgs : EventArgs
    {
        public ImageViewState ViewState { get; }

        public TaskProgressingEventArgs(ImageViewState viewState)
        {
            this.ViewState = viewState;
        }

    }

}
