using System.Diagnostics;

ProcessModule? module = Process.GetCurrentProcess().MainModule;
if (module is null)
    throw new NullReferenceException("MainModule of current process is null");
Console.WriteLine($"file-distributor version {module.FileVersionInfo.FileVersion}\n");

const long GigabyteSize = 1024L * 1024L * 1024L;

int sizeGB = int.Parse(args[0]);
long maxSizeBytes = sizeGB * GigabyteSize;
string folderA = args[1];
string folderB = args[2];


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

long currentBytes = 0;
for (int i =0;i< files.Count; i++)
{
    file_distributor.File currentFile = files[i];
    currentBytes += currentFile.Info.Length;
    if (currentBytes > maxSizeBytes)
    {
        // move to folder B
        string newPath = Path.Combine(folderB, currentFile.RelativePath);
        if (File.Exists(newPath))
            continue;
        Console.WriteLine($"Moving {currentFile.Info.FullName} to Folder B");
        if (!Directory.Exists(Path.GetDirectoryName(newPath)))
            Directory.CreateDirectory(Path.GetDirectoryName(newPath));
        currentFile.Info.MoveTo(newPath, false);
    }
    else
    {
        // move to folder A
        string newPath = Path.Combine(folderA, currentFile.RelativePath);
        if (File.Exists(newPath))
            continue;
        if (!Directory.Exists(Path.GetDirectoryName(newPath)))
            Directory.CreateDirectory(Path.GetDirectoryName(newPath));
        Console.WriteLine($"Moving {currentFile.Info.FullName} to Folder A");
        currentFile.Info.MoveTo(newPath, false);
    }
}


List<string> GetFiles(string path)
{
    List<string> files = new List<string>();
    files.AddRange(Directory.GetFiles(path));
    foreach (string subDir in Directory.GetDirectories(path))
        files.AddRange(GetFiles(subDir));
    return files;
}