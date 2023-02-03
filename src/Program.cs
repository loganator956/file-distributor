using System.Diagnostics;
using System.Reflection;
using System.Xml;
using file_distributor;

// Version Printing
Version appVersion = Assembly.GetExecutingAssembly().GetName().Version ?? new Version(-1, -1);
string versionString = $"V{appVersion.Major}.{appVersion.Minor}.{appVersion.Build}.{appVersion.Revision}";
Console.WriteLine($"file-distributor version {versionString}\n");

// Data size conversions
const long GigabyteSize = 1024L * 1024L * 1024L;

// Retrieve arguments
List<Argument> arguments = ArgumentProcessor.Process(args);

string aPath = "";
string bPath = "";
string argSize = "";

// Try get arguments

if (bool.TryParse(arguments.Find(x => x.Name == "help").Value, out _))
{
    PrintHelp();
    Environment.Exit(0);
}

aPath = arguments.Find(x => x.Name == "folder-a").Value;
Argument? _tempArg;
ArgumentProcessor.TryGetEnvVariable("FD_FOLDER_A", out _tempArg);
if (_tempArg is not null)
    aPath = _tempArg.Value.Value;

if (string.IsNullOrEmpty(aPath))
{
    PrintInColour("--folder-a not set", ConsoleColor.Red);
    Environment.Exit(1);
}


bPath = arguments.Find(x => x.Name == "folder-b").Value;
ArgumentProcessor.TryGetEnvVariable("FD_FOLDER_B", out _tempArg);
if (_tempArg is not null)
    bPath = _tempArg.Value.Value;

if (string.IsNullOrEmpty(bPath))
{
    PrintInColour("--folder-b not set", ConsoleColor.Red);
    Environment.Exit(1);
}


argSize = arguments.Find(x => x.Name == "size").Value;
ArgumentProcessor.TryGetEnvVariable("FD_SIZE", out _tempArg);
if (_tempArg is not null)
    argSize = _tempArg.Value.Value;

if (string.IsNullOrEmpty(argSize))
{
    PrintInColour("--size not set", ConsoleColor.Red);
    Environment.Exit(1);
}

// Optional Data
bool enableMonitorMode = bool.TryParse((arguments.Find(x => x.Name == "monitor")).Value, out _);
ArgumentProcessor.TryGetEnvVariable("FD_MONITOR_MODE", out _tempArg);
if (_tempArg is not null)
    bool.TryParse(_tempArg.Value.Value, out enableMonitorMode);

// ignore information
List<string> ignoredKeywords = new List<string>();
foreach(Argument arg in arguments.FindAll(x => x.Name == "ignore-keyword"))
{
    ignoredKeywords.Add(arg.Value);
}
List<string> ignoredFiles = new List<string>();
foreach (Argument arg in arguments.FindAll(x => x.Name == "ignore-file"))
{
    ignoredFiles.Add(arg.Value);
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
    DistributeFiles();
}

while (enableMonitorMode)
{
    DistributeFiles();
    Thread.Sleep(300000);
}

// Print config summary
Console.WriteLine(@$"Config:
Folder A: {aPath}
Folder B: {bPath}
Size of A: {sizeGB}");

void DistributeFiles()
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
        string relPath = file.Replace(folderA, "", StringComparison.CurrentCultureIgnoreCase).TrimStart('\\');
        files.Add(new(file, relPath));
    }
    List<string> fileListB = new List<string>();
    fileListB.AddRange(GetFiles(folderB));
    foreach (string file in fileListB)
    {
        string relPath = file.Replace(folderB, "", StringComparison.CurrentCultureIgnoreCase).TrimStart('\\');
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
        if (!CheckPath(currentFile.RelativePath, ignoredKeywords, ignoredFiles))
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

bool CheckPath(string path, List<string> keywords, List<string> fileNames)
{
    foreach(string keyword in keywords)
    {
        if (path.Contains(keyword))
            return false;
    }

    foreach(string name in fileNames)
    {
        if (Path.GetFileName(path).Contains(name))
            return false;
    }

    return true;
}

void TryMoveFile(FileInfo file, string destinationPath, bool isATarget)
{
    string parentDirectoryPath = Path.GetDirectoryName(destinationPath) ?? string.Empty;
    if (!Directory.Exists(parentDirectoryPath) && !string.IsNullOrEmpty(parentDirectoryPath))
        Directory.CreateDirectory(parentDirectoryPath);
    Console.WriteLine($"[{(isATarget ? "B -> A" : "A -> B")}] {file.FullName} TO {destinationPath}");
    file.MoveTo(destinationPath, false);
}

List<string> GetFiles(string path)
{
    List<string> files = new List<string>();
    files.AddRange(Directory.GetFiles(path));
    foreach (string subDir in Directory.GetDirectories(path))
        files.AddRange(GetFiles(subDir));
    return files;
}

void PrintInColour(string message, ConsoleColor colour)
{
    ConsoleColor prevColour = Console.ForegroundColor;
    Console.ForegroundColor = colour;
    Console.WriteLine(message);
    Console.ForegroundColor = prevColour;
}

void PrintHelp()
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
-h      --help              Display this help page");
}
