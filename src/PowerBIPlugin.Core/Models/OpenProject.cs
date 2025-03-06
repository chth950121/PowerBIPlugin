public class OpenProject
{
    public string? ProjectName { get; set; } // Use nullable if they can be null
    public string? FilePath { get; set; }
    public int? Port { get; set; }
    public string? WorkspacePath { get; set; }
    public DateTime LastModified { get; set; }

    public OpenProject(string? projectName, string? filePath, int? port, string? workspacePath, DateTime lastModified)
    {
        ProjectName = projectName;
        FilePath = filePath;
        Port = port;
        WorkspacePath = workspacePath;
        LastModified = lastModified;
    }
}
