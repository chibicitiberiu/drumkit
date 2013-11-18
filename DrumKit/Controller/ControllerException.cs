using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrumKit
{
    class ControllerException : Exception
    {
        public ControllerException() :
            base() { }

        public ControllerException(string message) :
            base(message) { }

        public ControllerException(string message, Exception innerException) :
            base(message, innerException) { }
    }
}
