using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.DoujinshiDownloader.Schedulers
{
    [Serializable]
    public class TaskInvalidStateException : Exception
    {
        public TaskInvalidStateException()
        {

        }

        public TaskInvalidStateException(string message) : base(message)
        {

        }

        public TaskInvalidStateException(string message, Exception innerException) : base(message, innerException)
        {

        }

    }

}
