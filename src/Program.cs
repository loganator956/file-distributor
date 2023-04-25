using System.Reflection;
using Mono.Options;
using file_distributor.Debugging;

namespace file_distributor
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // Version Printing
            PrintVersion();

            // Set defaults values
            const int MonitorWaitSecondsDefault = 300;

            string aPath, bPath;
            int size;
            int monitorWaitSeconds = MonitorWaitSecondsDefault;
            
            // ignore information
            List<string> ignoredKeywords = new List<string>();

            string ignoredFilePath = string.Empty;
            bool showHelp = false;
            // Get optoions
            OptionSet options = new()
            {
                { "h|help", "show this message and exit", v => showHelp = v != null },
                { "a|folder-a", "specify path for folder A.", v=> aPath = v }
            };

            List<string> extra;
            try
            {
                extra = options.Parse(args);
                ;
            }
            catch (OptionException e)
            {
                Console.WriteLine("e");
                Console.WriteLine("Try file-distributor --help for more information");
            }
            /*var p = new OptionSet() {
                { "n|name=", "the {NAME} of someone to greet.",
       v => names.Add (v) },
    { "r|repeat=",
       "the number of {TIMES} to repeat the greeting.\n" +
          "this must be an integer.",
        (int v) => repeat = v },
    { "v", "increase debug message verbosity",
       v => { if (v != null) ++verbosity; } },
    { "h|help",  "show this message and exit",
       v => show_help = v != null },
};*/

            bool enableMonitorMode = false;

            // call once if normal mode
            if (!enableMonitorMode)
            {
                //distributor.DistributeFiles();
            }
            // call repeatedly in monitor mode
            while (enableMonitorMode)
            {
                //distributor.DistributeFiles();
                Thread.Sleep(monitorWaitSeconds * 1000);
            }
        }

        static void PrintHelp()
        {
            Console.WriteLine(@"file-distributor help
REQUIRED OPTIONS
SHORT   LONG            DESC
-a      --folder-a      Path for folder A
-b      --folder-b      Path for folder B
-s      --size          Maximum sized for folder A

OPTIONAL OPTIONS
SHORT   LONG                DESC
-m      --monitor           Monitor mode
##-i      --ignore-keyword    Specify a keyword to ignore (Can be used multiple times)
-h      --help              Display this help page
-f      --ignore-file       Specifies a path to a file containing ignored keywords. (Each line is a separate keyword. Ignores lines starting with #)
##-v      --verbose           Specifies the verbosity level. (Either by calling argument multiple times or specifying a value for argument)
-w      --wait-interval     Specifies the amount of time (seconds) between running the distribute files in ""monitor"" mode");
        }

        static void PrintVersion()
        {
            Version appVersion = Assembly.GetExecutingAssembly().GetName().Version ?? new Version(-1, -1);
            string versionString = $"V{appVersion.Major}.{appVersion.Minor}.{appVersion.Build}.{appVersion.Revision}";
            Console.WriteLine($"file-distributor version {versionString}\n");
        }
    }
}