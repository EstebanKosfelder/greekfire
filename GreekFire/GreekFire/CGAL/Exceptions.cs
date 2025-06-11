using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CGAL
{

  


    public class ArithmeticOverflowException : ArithmeticException
    {
        public ArithmeticOverflowException() : base() { }
        public ArithmeticOverflowException(string message) : base(message) { }
        public ArithmeticOverflowException(string message, Exception innerException) : base(message, innerException) { }

    }

    public class IndeterminateValueException : Exception
    {
        public IndeterminateValueException() : base() { }
        public IndeterminateValueException(string message):base(message) { }
        public IndeterminateValueException(string message, Exception innerException):base(message,innerException) { }

    }

    public class NotValidPoligonException : Exception
    {
        public NotValidPoligonException() : base() { }
        public NotValidPoligonException(string message) : base(message) { }
        public NotValidPoligonException(string message, Exception innerException) : base(message, innerException) { }

    }
    
}
