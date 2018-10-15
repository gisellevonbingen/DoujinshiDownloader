using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.DoujinshiDownloader.Schedulers
{
    [Flags]
    public enum TaskState : byte
    {
        NotStarted = 1 << 0,
        Starting = 1 << 1,
        Running = 1 << 2,
        Canceling = 1 << 3,
        Cancelled = 1 << 4,
        Excepted = 1 << 5,
        Completed = 1 << 6,
    }

}
