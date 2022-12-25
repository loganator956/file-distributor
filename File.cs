namespace file_distributor
{
    public struct File
    {
        public File(string FullPath, string RelativePath)
        {
            this.Info = new(FullPath);
            this.RelativePath = RelativePath;
        }
        public FileInfo Info;
        public string RelativePath;
    }
}
