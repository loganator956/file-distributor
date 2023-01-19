using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace file_distributor
{
    internal static class ArgumentProcessor
    {
        public static Dictionary<string, object> Process(string[] args)
        {
            Dictionary<string,object> arguments= new Dictionary<string,object>();

            List<string> parts = new List<string>();

            // Expand and split the arguments into "parts"
            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i];
                if (arg.StartsWith('-'))
                {
                    arg = arg.TrimStart('-');
                    foreach(char c in arg)
                    {
                        string? mapped;
                        if (ArgumentMapper.Map.TryGetValue(c.ToString(), out mapped))
                        {
                            parts.Add("--" + mapped);
                        }
                        else
                        {
                            throw new ArgumentMappingException($"Couldn't find mapped argument of short name {c}");
                        }
                    }
                }
                else
                {
                    parts.Add(arg);
                }
            }

            // Convert arguments list to a dictionary
            for (int i = 0; i < parts.Count; i++)
            {
                object value = true;
                string argument = parts[i];
                if (!argument.StartsWith("--"))
                {
                    throw new ArgumentException("Wasn't expecting a value with no key");
                }

                if (i + 1 < parts.Count)
                {
                    // not last index
                    string nextArgument = parts[i + 1];
                    if (!nextArgument.StartsWith("--"))
                    {
                        value = nextArgument;
                        i += 1;
                    }
                }

                arguments.Add(argument.TrimStart('-'), value);
            }

            return arguments;
        }
    }
}
