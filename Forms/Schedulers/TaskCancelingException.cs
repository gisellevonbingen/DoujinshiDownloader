using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.DoujinshiDownloader.Schedulers
{
    [Serializable]
    public class TaskCancelingException : Exception
    {
        public TaskCancelingException() { }
        public TaskCancelingException(string message) : base(message) { }
        public TaskCancelingException(string message, Exception inner) : base(message, inner) { }
        protected TaskCancelingException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

}
