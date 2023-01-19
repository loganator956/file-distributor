using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace file_distributor
{
    public class ArgumentMappingException : Exception
    {
        public ArgumentMappingException()
        {
        }

        public ArgumentMappingException(string message)
            : base(message)
        {
        }

        public ArgumentMappingException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
