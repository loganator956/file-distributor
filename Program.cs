using System.Diagnostics;
using file_distributor;

// Version Printing
ProcessModule? module = Process.GetCurrentProcess().MainModule;
if (module is null)
    throw new NullReferenceException("MainModule of current process is null");
Console.WriteLine($"file-distributor version {module.FileVersionInfo.FileVersion}\n");

// Data size conversions
const long GigabyteSize = 1024L * 1024L * 1024L;

// Retrieve arguments
Dictionary<string,object> arguments = ArgumentProcessor.Process(args);

string aPath = (string)arguments["folder-a"];
string bPath = (string)arguments["folder-b"];
string argSize = (string)arguments["size"];

;
// Check arguments

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

// Set variables
long maxSizeBytes = sizeGB * GigabyteSize;
string folderA = aPath;
string folderB = bPath;

// Gather files and their FileInfos
List<file_distributor.File> files = new List<file_distributor.File>();
List<string> fileList = new List<string>();
fileList.AddRange(GetFiles(folderA));
fileList.AddRange(GetFiles(folderB));
foreach (string file in fileList)
{
    string relPath = file.Replace(folderA, "", StringComparison.CurrentCultureIgnoreCase).TrimStart('\\');
    relPath = relPath.Replace(folderB, "", StringComparison.CurrentCultureIgnoreCase).TrimStart('\\');
    files.Add(new(file, relPath));
}

// sort files by modified date and get the top x amount that fits into sizeGB
// sort files
files.Sort((a, b) => a.Info.LastWriteTime.CompareTo(b.Info.LastWriteTime));
files.Reverse(); // make descending order

long currentBytes = 0; // keeps track of the amount of bytes assigned to folder A
for (int i =0;i< files.Count; i++)
{
    file_distributor.File currentFile = files[i];
    currentBytes += currentFile.Info.Length;
    string newPath = string.Empty;
    if (currentBytes > maxSizeBytes)
    {
        // move to folder B
        newPath = Path.Combine(folderB, currentFile.RelativePath);
        Console.WriteLine($"Moving {currentFile.Info.FullName} to Folder B");
    }
    else
    {
        // move to folder A
        newPath = Path.Combine(folderA, currentFile.RelativePath);
        Console.WriteLine($"Moving {currentFile.Info.FullName} to Folder A");
    }
    // Attempt to move file, WITHOUT overwrite
    if (string.IsNullOrEmpty(newPath) || System.IO.File.Exists(newPath))
        continue;
    TryMoveFile(currentFile.Info, newPath);
}

void TryMoveFile(FileInfo file, string destinationPath)
{
    string parentDirectoryPath = Path.GetDirectoryName(destinationPath) ?? string.Empty;
    if (!Directory.Exists(Path.GetDirectoryName(parentDirectoryPath)) && !string.IsNullOrEmpty(parentDirectoryPath))
        Directory.CreateDirectory(parentDirectoryPath);
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