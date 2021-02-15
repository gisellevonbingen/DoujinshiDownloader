using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.DoujinshiDownloader.Schedulers
{
    [Serializable]
    public class TaskCancelingException : Exception
    {
        public TaskCancelingException()
        {

        }

        public TaskCancelingException(string message) : base(message)
        {

        }

        public TaskCancelingException(string message, Exception inner) : base(message, inner)
        {

        }

        [SecuritySafeCritical]
        protected TaskCancelingException(SerializationInfo info, StreamingContext context) : base(info, context)
        {

        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }

    }

}
