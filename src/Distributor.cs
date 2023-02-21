namespace file_distributor
{
    internal class Distributor
    {
        public string aPath { get; private set; }
        public string bPath { get; private set; }
        public int sizeGB { get; private set; }
        public List<string> ignoredKeywords { get; private set; }

        public Distributor(string a, string b, int size)
        {
            aPath = a;
            if (!Directory.Exists(a))
                throw new DirectoryNotFoundException($"Couldn't find folder a at {a}");
            bPath = b;
            if (!Directory.Exists(b))
                throw new DirectoryNotFoundException($"Couldn't find folder b at {b}");
            sizeGB = size;
            if (sizeGB <= 0)
                throw new ArgumentOutOfRangeException("sizeGB", "sizeGB is less than, or equal to, 0");
            ignoredKeywords = new List<string>();
        }

        public void DistributeFiles()
        {
            const long GigabyteSize = 1024L * 1024L * 1024L;
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
                Console.WriteLine($"[{(isATarget ? "B -> A" : "A -> B")}] {file.FullName} TO {destinationPath}");
                file.MoveTo(destinationPath, false);
            }

            static List<string> GetFiles(string path)
            {
                Console.WriteLine($"Discovering Files: {path}");
                List<string> files = new List<string>();
                files.AddRange(Directory.GetFiles(path));
                foreach (string subDir in Directory.GetDirectories(path))
                    files.AddRange(GetFiles(subDir));
                return files;
            }
        }

        public override string ToString()
        {
            return $@"Distributor:
aPath = {aPath}
bPath = {bPath}
size = {sizeGB} GB";
        }
    }
}