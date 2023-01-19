using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace file_distributor
{
    internal struct Argument
    {
        public string Name { get; private set; }
        public object Value { get; private set; }

        public Argument(string name, object value)
        {
            Name = name; Value = value;
        }
    }
}
