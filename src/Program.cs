using System.Reflection;
using file_distributor.Arguments;

namespace file_distributor
{
    internal class Program
    {
        const long GigabyteSize = 1024L * 1024L * 1024L;

        static void Main(string[] args)
        {
            // Version Printing
            Version appVersion = Assembly.GetExecutingAssembly().GetName().Version ?? new Version(-1, -1);
            string versionString = $"V{appVersion.Major}.{appVersion.Minor}.{appVersion.Build}.{appVersion.Revision}";
            Console.WriteLine($"file-distributor version {versionString}\n");

            // Data size conversions

            // Retrieve arguments
            ArgumentsList newArgs = new ArgumentsList(args);
            List<Argument> arguments = ArgumentParser.Process(args);
            const int MonitorWaitSecondsDefault = 300;

            // Try get arguments
            if (newArgs.TryGetValue("help", "", out _))
            {
                PrintHelp();
                Environment.Exit(0);
            }

            // TODO: Change to newArgs.GetValue instead of arguments.Find

            string aPath = newArgs.GetValue("folder-a", "FD_FOLDER_A", string.Empty);
            if (string.IsNullOrEmpty(aPath))
            {
                PrintInColour("--folder-a not set", ConsoleColor.Red);
                Environment.Exit(1);
            }
            string bPath = newArgs.GetValue("folder-b", "FD_FOLDER_B", string.Empty);
            if (string.IsNullOrEmpty(bPath))
            {
                PrintInColour("--folder-b not set", ConsoleColor.Red);
                Environment.Exit(1);
            }
            string argSize = newArgs.GetValue("size", "FD_SIZE", string.Empty);
            if (string.IsNullOrEmpty(argSize))
            {
                PrintInColour("--size not set", ConsoleColor.Red);
                Environment.Exit(1);
            }

            // Optional Data
            bool enableMonitorMode = false;
            if (!bool.TryParse(newArgs.GetValue("monitor", "FD_MONITOR_MODE", "false"), out enableMonitorMode))
            {
                PrintInColour("Unknown value for monitor mode", ConsoleColor.Red);
                Environment.Exit(1);
            }

            int monitorWaitSeconds = MonitorWaitSecondsDefault;
            if (!int.TryParse(newArgs.GetValue("wait-interval", "FD_MONITOR_WAIT_INTERVAL", MonitorWaitSecondsDefault.ToString()), out monitorWaitSeconds))
            {
                PrintInColour("Unknown value for monitor wait seconds", ConsoleColor.Red);
                Environment.Exit(1);
            }
            if (monitorWaitSeconds <= 0)
                monitorWaitSeconds = MonitorWaitSecondsDefault;


            // ignore information
            List<string> ignoredKeywords = new List<string>();

            string ignoredFilePath = string.Empty;
            if (newArgs.TryGetValue("ignore-file", "FD_IGNORE_FILE", out ignoredFilePath))
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
            // Verify argument data

            int sizeGB = -1;
            if (!int.TryParse(argSize, out sizeGB))
            {
                PrintInColour($"Cannot parse argument 0 ({argSize}) to int. Ensure you only entering a plain integer number, eg '2'", ConsoleColor.Red);
                Environment.Exit(1);
            }
            if (!Directory.Exists(aPath))
            {
                PrintInColour($"Cannot find folder A ({aPath})", ConsoleColor.Red);
                Environment.Exit(1);
            }
            if (!Directory.Exists(bPath))
            {
                PrintInColour($"Cannot find folder B ({bPath})", ConsoleColor.Red);
                Environment.Exit(1);
            }
            if (sizeGB <= 0)
            {
                PrintInColour($"Warning: SizeGB is <=0. This will mean all files will be sent to folder B", ConsoleColor.Yellow);
                Thread.Sleep(1000);
            }

            if (!enableMonitorMode)
            {
                DistributeFiles(aPath, bPath, sizeGB, ignoredKeywords);
            }

            while (enableMonitorMode)
            {
                DistributeFiles(aPath, bPath, sizeGB, ignoredKeywords);
                Thread.Sleep(monitorWaitSeconds * 1000);
            }

            // Print config summary
            Print(@$"Config:
Folder A: {aPath}
Folder B: {bPath}
Size of A: {sizeGB}");
            foreach (string keyword in ignoredKeywords)
            {
                Print($"Ignored Keyword: {keyword}");
            }

        }

        static void DistributeFiles(string aPath, string bPath, int sizeGB, List<string> ignoredKeywords)
        {
            // Set variables
            long maxSizeBytes = sizeGB * GigabyteSize;
            string folderA = aPath;
            string folderB = bPath;

            // Gather files and their FileInfos
            List<file_distributor.File> files = new List<file_distributor.File>();
            List<string> fileListA = new List<string>();
            fileListA.AddRange(GetFiles(folderA));
            foreach (string file in fileListA)
            {
                string relPath = file.Replace(folderA, "", StringComparison.CurrentCultureIgnoreCase).TrimStart('\\').TrimStart('/');
                files.Add(new(file, relPath));
            }
            List<string> fileListB = new List<string>();
            fileListB.AddRange(GetFiles(folderB));
            foreach (string file in fileListB)
            {
                string relPath = file.Replace(folderB, "", StringComparison.CurrentCultureIgnoreCase).TrimStart('\\').TrimStart('/');
                files.Add(new(file, relPath));
            }

            // sort files by modified date and get the top x amount that fits into sizeGB
            // sort files
            files.Sort((a, b) => a.Info.LastWriteTime.CompareTo(b.Info.LastWriteTime));
            files.Reverse(); // make descending order

            long currentBytes = 0; // keeps track of the amount of bytes assigned to folder A
            for (int i = 0; i < files.Count; i++)
            {
                file_distributor.File currentFile = files[i];
                currentBytes += currentFile.Info.Length;
                string newPath = string.Empty;
                bool isATarget = false;
                if (!CheckPath(currentFile.RelativePath, ignoredKeywords))
                    continue;
                if (currentBytes > maxSizeBytes)
                {
                    // move to folder B
                    newPath = Path.Combine(folderB, currentFile.RelativePath);
                    //Console.WriteLine($"Moving {currentFile.Info.FullName} to Folder B");
                    isATarget = false;
                }
                else
                {
                    // move to folder A
                    newPath = Path.Combine(folderA, currentFile.RelativePath);
                    isATarget = true;
                    //Console.WriteLine($"Moving {currentFile.Info.FullName} to Folder A");
                }
                // Attempt to move file, WITHOUT overwrite
                if (string.IsNullOrEmpty(newPath) || System.IO.File.Exists(newPath))
                    continue;
                TryMoveFile(currentFile.Info, newPath, isATarget);
            }
        }

        static bool CheckPath(string path, List<string> keywords)
        {
            foreach (string keyword in keywords)
            {
                if (path.Contains(keyword))
                    return false;
            }

            return true;
        }

        static void TryMoveFile(FileInfo file, string destinationPath, bool isATarget)
        {
            string parentDirectoryPath = Path.GetDirectoryName(destinationPath) ?? string.Empty;
            if (!Directory.Exists(parentDirectoryPath) && !string.IsNullOrEmpty(parentDirectoryPath))
                Directory.CreateDirectory(parentDirectoryPath);
            Print($"[{(isATarget ? "B -> A" : "A -> B")}] {file.FullName} TO {destinationPath}");
            file.MoveTo(destinationPath, false);
        }

        static List<string> GetFiles(string path)
        {
            Print($"Discovering Files: {path}");
            List<string> files = new List<string>();
            files.AddRange(Directory.GetFiles(path));
            foreach (string subDir in Directory.GetDirectories(path))
                files.AddRange(GetFiles(subDir));
            return files;
        }

        static void PrintInColour(string message, ConsoleColor colour)
        {
            ConsoleColor prevColour = Console.ForegroundColor;
            Console.ForegroundColor = colour;
            Console.WriteLine(message);
            Console.ForegroundColor = prevColour;
        }

        static void Print(string message)
        {
            Console.WriteLine(message);
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
-i      --ignore-keyword    Specify a keyword to ignore (Can be used multiple times)
-h      --help              Display this help page
-f      --ignore-file       Specifies a path to a file containing ignored keywords. (Each line is a separate keyword. Ignores lines starting with #)
-v      --verbose           Specifies the verbosity level. (Either by calling argument multiple times or specifying a value for argument)
-w      --wait-interval     Specifies the amount of time (seconds) between running the distribute files in ""monitor"" mode");
        }
    }
}