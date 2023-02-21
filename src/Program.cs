using System.Reflection;
using file_distributor.Arguments;
using file_distributor.Debugging;

namespace file_distributor
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // Version Printing
            PrintVersion();

            // Retrieve arguments
            ArgumentsList arguments = new ArgumentsList(args);

            // Set defaults values
            const int MonitorWaitSecondsDefault = 300;

            // Try get arguments
            #region Argument Retrieval
            if (arguments.TryGetValue("help", "", out _))
            {
                PrintHelp();
                Environment.Exit(0);
            }

            string aPath = arguments.GetValue("folder-a", "FD_FOLDER_A", string.Empty);
            if (string.IsNullOrEmpty(aPath))
            {
                Debugger.PrintInColour("--folder-a not set", ConsoleColor.Red);
                Environment.Exit(1);
            }
            string bPath = arguments.GetValue("folder-b", "FD_FOLDER_B", string.Empty);
            if (string.IsNullOrEmpty(bPath))
            {
                Debugger.PrintInColour("--folder-b not set", ConsoleColor.Red);
                Environment.Exit(1);
            }
            string argSize = arguments.GetValue("size", "FD_SIZE", string.Empty);
            if (string.IsNullOrEmpty(argSize))
            {
                Debugger.PrintInColour("--size not set", ConsoleColor.Red);
                Environment.Exit(1);
            }

            // Optional Data
            bool enableMonitorMode = false;
            if (!bool.TryParse(arguments.GetValue("monitor", "FD_MONITOR_MODE", "false"), out enableMonitorMode))
            {
                Debugger.PrintInColour("Unknown value for monitor mode", ConsoleColor.Yellow);
                Environment.Exit(1);
            }

            int monitorWaitSeconds = MonitorWaitSecondsDefault;
            if (!int.TryParse(arguments.GetValue("wait-interval", "FD_MONITOR_WAIT_INTERVAL", MonitorWaitSecondsDefault.ToString()), out monitorWaitSeconds))
            {
                Debugger.PrintInColour("Unknown value for monitor wait seconds", ConsoleColor.Yellow);
                Environment.Exit(1);
            }
            if (monitorWaitSeconds <= 0)
                monitorWaitSeconds = MonitorWaitSecondsDefault;

            // ignore information
            List<string> ignoredKeywords = new List<string>();

            string ignoredFilePath = string.Empty;
            if (arguments.TryGetValue("ignore-file", "FD_IGNORE_FILE", out ignoredFilePath))
            {
                // read ignore file
                if (System.IO.File.Exists(ignoredFilePath))
                {
                    string[] lines = System.IO.File.ReadAllLines(ignoredFilePath);
                    foreach (string line in lines)
                    {
                        if (line.StartsWith('#'))
                            continue;
                        ignoredKeywords.Add(line);
                    }
                }
                else
                {
                    throw new FileNotFoundException($"Cannot find ignore file at {ignoredFilePath}", ignoredFilePath);
                }
            }
            #endregion

            // Convert arguments into their correct datatypes?
            int size = 0;
            if (!int.TryParse(argSize, out size))
            {
                Debugger.PrintInColour($"Unknown value for arg size {argSize}", ConsoleColor.Red);
                Environment.Exit(1);
            }
            // create distributor
            Distributor distributor = new Distributor(aPath, bPath, size);

            // call once if normal mode
            if (!enableMonitorMode)
            {
                distributor.DistributeFiles();
            }
            // call repeatedly in monitor mode
            while (enableMonitorMode)
            {
                distributor.DistributeFiles();
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