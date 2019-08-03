using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.DoujinshiDownloader.Schedulers
{
    [Flags]
    public enum ViewState : byte
    {
        StandBy = 1 << 0,
        Downloading = 1 << 1,
        Success = 1 << 2,
        Exception = 1 << 3,
    }

}
