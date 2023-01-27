using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace file_distributor
{
    internal static class ArgumentProcessor
    {
        public static bool TryGetEnvVariable(string name, out Argument? arg)
        {
            string? val = Environment.GetEnvironmentVariable(name);
            if (val != null)
            {
                arg = new Argument(name, val);
                return true;
            }
            arg = null;
            return false;
        }

        public static List<Argument> Process(string[] args)
        {
            List<Argument> arguments= new List<Argument>();

            List<string> parts = new List<string>();

            // Expand and split the arguments into "parts"
            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i];
                if (arg.StartsWith('-') && !arg.StartsWith("--"))
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
                string value = "true";
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

                arguments.Add(new Argument(argument.TrimStart('-'), value));
            }

            return arguments;
        }
    }
}
