using System.Diagnostics;
using System.Xml;
using file_distributor;

// Version Printing
ProcessModule? module = Process.GetCurrentProcess().MainModule;
if (module is null)
    throw new NullReferenceException("MainModule of current process is null");
Console.WriteLine($"file-distributor version {module.FileVersionInfo.FileVersion}\n");

// Data size conversions
const long GigabyteSize = 1024L * 1024L * 1024L;

// Retrieve arguments
List<Argument> arguments = ArgumentProcessor.Process(args);

string aPath = "";
string bPath = "";
string argSize = "";

// Try get arguments

try 
{
    aPath = arguments.Find(x => x.Name == "folder-a").Value; 
}
catch (KeyNotFoundException)
{ 
    PrintInColour("--folder-a not set", ConsoleColor.Red);
    Environment.Exit(1);
}
try
{
    bPath = arguments.Find(x => x.Name == "folder-b").Value; 
}
catch (KeyNotFoundException)
{
    PrintInColour("--folder-b not set", ConsoleColor.Red);
    Environment.Exit(1);
}
try
{
    argSize = arguments.Find(x => x.Name == "size").Value;
}
catch( KeyNotFoundException)
{
    PrintInColour("--size not set", ConsoleColor.Red);
    Environment.Exit(1);
}

// Optional Data
bool enableMonitorMode = bool.TryParse((arguments.Find(x => x.Name == "monitor")).Value, out _);

// ignore information
List<string> ignoredKeywords = new List<string>();
foreach(Argument arg in arguments.FindAll(x => x.Name == "ignore-keyword"))
{
    ignoredKeywords.Add(arg.Value);
}
List<string> ignoredFiles = new List<string>();
foreach(Argument arg in arguments.FindAll(x => x.Name == "ignore-file"))
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
;
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
    if (!Directory.Exists(Path.GetDirectoryName(parentDirectoryPath)) && !string.IsNullOrEmpty(parentDirectoryPath))
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