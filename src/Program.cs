using System.Reflection;
using Mono.Options;
using file_distributor.Debugging;
using System.Text.RegularExpressions;
using System.Runtime.CompilerServices;

namespace file_distributor
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // Version Printing
            PrintVersion();

            string aPath = "", bPath = "", sizeString= "", sortModeString="latest";
            long size;
            bool showHelp = false;
            // Get optoions
            OptionSet options = new()
            {
                { "a=|folder-a", "specify path for folder A.", v=> aPath = v },
                { "b=|folder-b", "specify path for folder B.", v=> bPath = v },
                { "s=|size", "specify the maximum size of folder A", v => sizeString = v},
                { "m=|sort", "specify sort mode (Latest, Random)", v=> sortModeString = v },
                { "h|help", "show this message and exit", v => showHelp = v != null }
            };

            if (showHelp) 
            {
                options.WriteOptionDescriptions(Console.Out);
                return;
            }
            List<string> extra;
            try
            {
                extra = options.Parse(args);
            }
            catch (OptionException e)
            {
                Console.WriteLine("e");
                Console.WriteLine("Try file-distributor --help for more information");
            }
            size = ConvertSizeToBytes(sizeString);
            Console.WriteLine($@"File Distributor Properties:
A Path: {aPath}
B Path: {bPath}
Size of A Path {size} bytes ({sizeString})");
            try
            {
                Distributor dist = new Distributor(aPath, bPath, size, sortModeString);
                dist.DistributeFiles();
            }
            catch (DirectoryNotFoundException dirNotFound)
            {
                Console.WriteLine($"Couldn't find a directory: {dirNotFound.Message}");
                Environment.Exit(1);
            }
        }

        static void PrintVersion()
        {
            Version appVersion = Assembly.GetExecutingAssembly().GetName().Version ?? new Version(-1, -1);
            string versionString = $"V{appVersion.Major}.{appVersion.Minor}.{appVersion.Build}.{appVersion.Revision}";
            Console.WriteLine($"file-distributor version {versionString}\n");
        }

        static long ConvertSizeToBytes(string sizeString)
        {
            long byteSize = 0;
            Match match = Regex.Match(sizeString, @"(\d*\.?\d*)(\w{1,3})");
            double size = double.Parse(match.Groups[1].Value);
            // TODO: Make this bit nicer
            switch(match.Groups[2].Value.ToLower())
            {
                case "kb":
                    byteSize = (long)Math.Round(size * 1000);
                    break;
                case "kib":
                    byteSize = (long)Math.Round(size * 1024);
                    break;
                case "mb":
                    byteSize = (long)Math.Round(size * 1000L * 1000L);
                    break;
                case "mib":
                    byteSize = (long)Math.Round(size * 1024L * 1024L);
                    break;
                case "gb":
                    byteSize = (long)Math.Round(size * 1000L * 1000L * 1000L);
                    break;
                case "gib":
                    byteSize = (long)Math.Round(size * 1024L * 1024L * 1024L);
                    break;
                default:
                    throw new ArgumentException($"{sizeString} has unrecognised unit");
            }
            return byteSize;
        }
    }
}