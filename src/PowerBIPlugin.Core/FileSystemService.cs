namespace PowerBIPlugin
{
    public class FileSystemService
    {
        public string GetWorkspaceDirectory() =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft", "Power BI Desktop", "AnalysisServicesWorkspaces");

        public bool DirectoryExists(string path) => Directory.Exists(path);
        public string[] GetDirectories(string path) => Directory.GetDirectories(path);
        public bool FileExists(string path) => File.Exists(path);
        public string ReadFile(string path) => File.ReadAllText(path);
        public string GetHandleExecutablePath() => @"E:\github\PowerBIPlugin\support\handle.exe";
        public DateTime GetLastWriteTime(string path) => File.GetLastWriteTime(path);
    }
}
