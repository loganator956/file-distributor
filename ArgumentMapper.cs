using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace file_distributor
{
    internal static class ArgumentMapper
    {
        public static Dictionary<string, string> Map = new Dictionary<string, string>()
        {
            { "a", "folder-a" },
            { "b", "folder-b" },
            { "s", "size" },
            { "h", "help" }
        };
    }
}
