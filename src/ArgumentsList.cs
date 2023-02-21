namespace file_distributor.Arguments
{
    public class ArgumentsList
    {
        public Dictionary<string, string> ArgumentDict = new Dictionary<string, string>();

        public ArgumentsList(string[] args)
        {
            // Expand and split the arguments
            List<string> argumentParts = new List<string>();
            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i];
                if (arg.StartsWith("--"))
                {
                    argumentParts.Add(arg);
                }
                else if (arg.StartsWith('-'))
                {
                    arg = arg.TrimStart('-');
                    foreach (char c in arg)
                    {
                        string? mapped;
                        if (ArgumentMapper.Map.TryGetValue(c.ToString(), out mapped))
                        {
                            argumentParts.Add("--" + mapped);
                        }
                        else
                        {
                            Console.WriteLine($"Couldn't find mapped argument of short name {c}");
                        }
                    }
                }
                else
                {
                    // add any remaining arguments (Usually values and stuff)
                    argumentParts.Add(arg);
                }
            }

            // Parse arguments
            for (int i = 0; i < argumentParts.Count; i++)
            {
                string value = "true";
                string argument = argumentParts[i];
                if (!argument.StartsWith("--"))
                    throw new ArgumentException("Wasn't expecting a vlue with no key :(");

                if (i + 1 < argumentParts.Count)
                {
                    // not last index
                    string nextArg = argumentParts[i + 1];
                    if (!nextArg.StartsWith("--"))
                    {
                        // next item is a value
                        value = nextArg;
                        i++;
                    }
                }

                // apply the argument
                ArgumentDict.Add(argument.TrimStart('-'), value);
            }
        }

        public bool TryGetValue(string argName, string envVarName, out string value)
        {
            bool success = false;

            value = string.Empty;

            string? argVal;
            string? envVal;
            if (ArgumentDict.TryGetValue(argName, out argVal))
            {
                value = argVal;
                success = true;
            }
            else
            {
                // failed to get command line argument, try get environment variable, instead.
                envVal = Environment.GetEnvironmentVariable(envVarName);
                if (envVal is not null)
                {
                    value = envVal;
                    success = true;
                }
            }

            if (string.IsNullOrEmpty(value))
                Console.WriteLine($"Value of argument {argName} or environment variable {envVarName} could not be found?");

            return success;
        }

        public string GetValue(string argName, string envVarName, string defaultVal)
        {
            string value;
            if (TryGetValue(argName, envVarName, out value))
            {
                return value;
            }
            else
            {
                return defaultVal;
            }
        }
    }
}