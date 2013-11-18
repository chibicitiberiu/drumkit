using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrumKit.Repository
{
    class RepositoryException : Exception
    {
        public RepositoryException() : 
            base() { }

        public RepositoryException(string message) : 
            base(message) { }

        public RepositoryException(string message, Exception innerException) : 
            base(message, innerException) { }
    }
}
