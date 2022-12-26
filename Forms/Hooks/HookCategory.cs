using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.DoujinshiDownloader.Hooks
{
    [Flags]
    public enum HookCategory : byte
    {
        None = 0,
        Download = 1 << 0,
    }

}
