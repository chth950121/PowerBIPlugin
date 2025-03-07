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
        public string GetHandleExecutablePath()
        {
            string basePath = AppDomain.CurrentDomain.BaseDirectory; // Get app's base directory
            string relativePath = Path.Combine(basePath, "support", "handle.exe");

            return relativePath;
        }

        public DateTime GetLastWriteTime(string path) => File.GetLastWriteTime(path);
    }
}
