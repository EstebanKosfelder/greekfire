using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GFL {
    public class GFLException : Exception
    {
        public GFLException()
        {
        }

        public GFLException(string? message) : base(message)
        {
        }

        public GFLException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected GFLException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }

    public class HasNotRelevantEdge : GFLException
    {
        public HasNotRelevantEdge()
        {
        }

        public HasNotRelevantEdge(string? message) : base(message)
        {
        }

        public HasNotRelevantEdge(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected HasNotRelevantEdge(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
