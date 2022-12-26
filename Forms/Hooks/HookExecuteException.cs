using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.DoujinshiDownloader.Hooks
{
    [Serializable]
    public class HookExecuteException : Exception
    {
        public HookExecuteException() { }
        public HookExecuteException(string message) : base(message) { }
        public HookExecuteException(string message, Exception inner) : base(message, inner) { }
        protected HookExecuteException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }

    }

}
