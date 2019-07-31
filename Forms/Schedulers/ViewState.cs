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
        StandBy = 0,
        Downloading = 1 << 1,
        Complete = 1 << 2,
        Success = 1 << 3 | Complete,
        Exception = 1 << 4 | Complete,
    }

}
