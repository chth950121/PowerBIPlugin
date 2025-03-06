using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.AnalysisServices.AdomdClient;

namespace PowerBIPlugin
{
    public class PowerBIService
    {
        private readonly FileSystemService _fileSystemService;
        private readonly ProcessService _processService;

        public PowerBIService(FileSystemService fileSystemService, ProcessService processService)
        {
            _fileSystemService = fileSystemService;
            _processService = processService;
        }

        public List<OpenProject> GetOpenPowerBIProjects()
        {
            List<string> ports = GetPorts();
            List<string> openPBIXFiles = GetPBIXFiles();
            return GetMatchedPP(openPBIXFiles, ports);
        }

        public List<string> GetQueries(string port)
        {
            var queries = new List<string>();
            string connectionString = $"Provider=MSOLAP;Data Source=localhost:{port};";

            try
            {
                using (var conn = new AdomdConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT * FROM $SYSTEM.TMSCHEMA_PARTITIONS";

                    using (var cmd = new AdomdCommand(query, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string partitionName = reader["Name"].ToString();
                            string partitionQuery = reader["QueryDefinition"] != DBNull.Value ? reader["QueryDefinition"].ToString() : "N/A";

                            Logger.Log($"Partition: {partitionName}, Query: {partitionQuery}");
                            queries.Add($"{partitionName} : {partitionQuery}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Error retrieving queries: {ex.Message}");
                throw;
            }

            return queries;
        }

        public List<string> GetMeasures(string port)
        {
            var measures = new List<string>();
            string connectionString = $"Provider=MSOLAP;Data Source=localhost:{port};";

            try
            {
                using (var conn = new AdomdConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT * FROM $SYSTEM.TMSCHEMA_MEASURES";

                    using (var cmd = new AdomdCommand(query, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string measureName = reader["Name"].ToString();
                            string measureExpression = reader["Expression"] != DBNull.Value ? reader["Expression"].ToString() : "N/A";

                            Logger.Log($"Measure: {measureName}, DAX: {measureExpression}");
                            measures.Add($"{measureName} : {measureExpression}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Error retrieving measures: {ex.Message}");
                throw;
            }

            return measures;
        }

        private List<string> GetPorts()
        {
            var ports = new List<string>();
            string workspaceDir = _fileSystemService.GetWorkspaceDirectory();

            if (_fileSystemService.DirectoryExists(workspaceDir))
            {
                Logger.Log($"Checking workspace directory: {workspaceDir}");
                var directories = _fileSystemService.GetDirectories(workspaceDir);

                foreach (var dir in directories)
                {
                    string portFilePath = Path.Combine(dir, "Data", "msmdsrv.port.txt");

                    if (_fileSystemService.FileExists(portFilePath))
                    {
                        string port = _fileSystemService.ReadFile(portFilePath)
                                        .Trim()
                                        .Replace("\0", ""); // Remove NULL characters

                        Logger.Log($"Found Power BI Port: {port}");
                        ports.Add(port);
                    }
                    else
                    {
                        Logger.Log($"File does not exist: {portFilePath}");
                    }
                }
            }

            Logger.Log($"Total Power BI Ports Found: {ports.Count}");
            return ports;
        }

        private List<string> GetPBIXFiles()
        {
            Logger.Log("Attempting to get currently open Power BI projects.");

            string handlePath = _fileSystemService.GetHandleExecutablePath();

            if (!_fileSystemService.FileExists(handlePath))
            {
                Logger.Log("Handle.exe not found. Download it from Microsoft Sysinternals.");
                return new List<string> { "Handle.exe not found. Download it from Microsoft Sysinternals." };
            }

            var openProjects = new List<string>();
            try
            {
                string output = _processService.ExecuteCommand(handlePath, "/c \"handle.exe -accepteula | findstr /i \".pbix\"\"");
                if (string.IsNullOrWhiteSpace(output))
                {
                    Logger.Log("No open PBIX files found.");
                    return new List<string> { "No open PBIX files found." };
                }

                var lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines)
                {
                    Match match = Regex.Match(line, @"[a-zA-Z]:\\.*?\.pbix", RegexOptions.IgnoreCase);
                    if (match.Success)
                    {
                        string filePath = match.Value;
                        openProjects.Add(filePath);
                        Logger.Log($"Found open PBIX file: {filePath}");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Error detecting open PBIX files: {ex.Message}");
                return new List<string> { $"Error detecting open PBIX files: {ex.Message}" };
            }

            Logger.Log("Open PBIX files retrieved successfully.");
            return openProjects;
        }

        private List<OpenProject> GetMatchedPP(List<string> openPBIXFiles, List<string> ports)
        {
            var matchedProjects = new List<OpenProject>();

            var pbixFileTimestamps = openPBIXFiles.ToDictionary(file => file, file => _fileSystemService.GetLastWriteTime(file));

            var sortedPBIX = pbixFileTimestamps.OrderByDescending(kvp => kvp.Value).ToList();
            var sortedPorts = ports.ToList();

            for (int i = 0; i < Math.Max(sortedPBIX.Count, sortedPorts.Count); i++)
            {
                matchedProjects.Add(new OpenProject(
                    i < sortedPBIX.Count ? Path.GetFileNameWithoutExtension(sortedPBIX[i].Key) : "Undefined",
                    i < sortedPBIX.Count ? sortedPBIX[i].Key : "Undefined",
                    i < sortedPorts.Count && int.TryParse(sortedPorts[i], out int port) ? (int?)port : null,
                    i < sortedPorts.Count ? $"Workspace_{sortedPorts[i]}" : "Undefined",
                    i < sortedPBIX.Count ? sortedPBIX[i].Value : DateTime.Now
                ));

                Logger.Log($"Matched PBIX: {sortedPBIX[i].Key ?? "Undefined"} with Port: {sortedPorts[i] ?? "Undefined"}");
            }

            return matchedProjects;
        }
    }
}
