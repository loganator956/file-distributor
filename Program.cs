using System.Diagnostics;

// Version Printing
ProcessModule? module = Process.GetCurrentProcess().MainModule;
if (module is null)
    throw new NullReferenceException("MainModule of current process is null");
Console.WriteLine($"file-distributor version {module.FileVersionInfo.FileVersion}\n");

// Data size conversions
const long GigabyteSize = 1024L * 1024L * 1024L;

// Check arguments
if (args.Length != 3)
{
    PrintInColour($"Incorrect number of arguments. Received: {args.Length}; Expected 3", ConsoleColor.Red);
    Environment.Exit(1);
}

int sizeGB = -1;
if (!int.TryParse(args[0], out sizeGB))
{
    PrintInColour($"Cannot parse argument 0 ({args[0]}) to int. Ensure you only entering a plain integer number, eg '2'", ConsoleColor.Red);
    Environment.Exit(1);
}
if (!Directory.Exists(args[1]))
{
    PrintInColour($"Cannot find folder A ({args[1]})", ConsoleColor.Red);
    Environment.Exit(1);
}
if (!Directory.Exists(args[2]))
{
    PrintInColour($"Cannot find folder B ({args[2]})", ConsoleColor.Red);
    Environment.Exit(1);
}
if (sizeGB <= 0)
{
    PrintInColour($"Warning: SizeGB is <=0. This will mean all files will be sent to folder B", ConsoleColor.Yellow);
    Thread.Sleep(1000);
}

// Set variables
long maxSizeBytes = sizeGB * GigabyteSize;
string folderA = args[1];
string folderB = args[2];

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
    if (string.IsNullOrEmpty(newPath) || File.Exists(newPath))
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