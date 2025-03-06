using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions; // Add this at the top

namespace PowerBIPlugin
{
    public class ProjectDetector
    {
        public static List<string> GetPorts()
        {
            List<string> ports = new List<string>();
            string workspaceDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                                                "Microsoft", "Power BI Desktop", "AnalysisServicesWorkspaces");

            if (Directory.Exists(workspaceDir))
            {
                Logger.Log($"Checking workspace directory: {workspaceDir}");
                if (!Directory.Exists(workspaceDir))
                {
                    Logger.Log("Workspace directory does not exist.");
                    return ports; // Exit early
                }

                var directories = Directory.GetDirectories(workspaceDir);
                Logger.Log($"Found {directories.Length} directories in workspaceDir.");
                
                foreach (var dirc in directories)
                {
                    Logger.Log($"Directory: {dirc}");
                }
                
                foreach (var dir in Directory.GetDirectories(workspaceDir))
                {
                    string portFilePath = Path.Combine(dir, "Data", "msmdsrv.port.txt");

                    if (File.Exists(portFilePath))
                    {
                        string port = File.ReadAllText(portFilePath).Trim();
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

        public static List<string> GetPBIXFiles()
        {
            Logger.Log("Attempting to get currently open Power BI projects.");

            string handlePath = @"E:\github\PowerBIPlugin\support\handle.exe";  

            if (!File.Exists(handlePath))
            {
                Logger.Log("Handle.exe not found. Download it from Microsoft Sysinternals.");
                return new List<string> { "Handle.exe not found. Download it from Microsoft Sysinternals." };
            }

            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c \"{handlePath} -accepteula | findstr /i \".pbix\"\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            List<string> openProjects = new List<string>();

            try
            {
                using (Process process = Process.Start(psi))
                {
                    if (process != null)
                    {
                        string output = process.StandardOutput.ReadToEnd();
                        string error = process.StandardError.ReadToEnd();
                        process.WaitForExit();

                        if (!string.IsNullOrEmpty(error))
                        {
                            Logger.Log($"Handle.exe Error: {error}");
                            return new List<string> { $"Handle.exe Error: {error}" };
                        }

                        if (string.IsNullOrWhiteSpace(output))
                        {
                            Logger.Log("No open PBIX files found.");
                            return new List<string> { "No open PBIX files found." };
                        }

                        string[] lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (string line in lines)
                        {
                            // Regex pattern to extract file path
                            Match match = Regex.Match(line, @"[a-zA-Z]:\\.*?\.pbix", RegexOptions.IgnoreCase);
                            if (match.Success)
                            {
                                string filePath = match.Value;
                                openProjects.Add(filePath);
                                Logger.Log($"Found open PBIX file: {filePath}");
                            }
                        }
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

        public static List<OpenProject> GetMatchedPP(List<string> openPBIXFiles, List<string> ports)
        {
            List<OpenProject> matchedProjects = new List<OpenProject>();

            // Get last modified timestamps of PBIX files
            Dictionary<string, DateTime> pbixFileTimestamps = new Dictionary<string, DateTime>();
            foreach (string pbixFile in openPBIXFiles)
            {
                Logger.Log($"pbix file: {pbixFile}");
                if (File.Exists(pbixFile))
                {
                    Logger.Log($"Time: {File.GetLastWriteTime(pbixFile)}");
                    pbixFileTimestamps[pbixFile] = File.GetLastWriteTime(pbixFile);
                }
            }

            // Sort PBIX files by last modified time (most recent first)
            var sortedPBIX = pbixFileTimestamps.OrderByDescending(kvp => kvp.Value).ToList();
            var sortedPorts = ports.ToList();

            Logger.Log($"PBIX: {sortedPBIX.Count}, Ports: {sortedPorts.Count}");

            // Match PBIX files with workspaces
            for (int i = 0; i < Math.Max(sortedPBIX.Count, sortedPorts.Count); i++)
            {
                if(i < sortedPBIX.Count)
                {
                    matchedProjects.Add(new OpenProject
                    {
                        ProjectName = Path.GetFileNameWithoutExtension(sortedPBIX[i].Key),
                        FilePath = sortedPBIX[i].Key,
                        Port = sortedPorts[i],
                        WorkspacePath = $"Workspace_{sortedPorts[i]}", // You may need to correctly set this value
                        LastModified = sortedPBIX[i].Value
                    });

                    Logger.Log($"Matched PBIX: {sortedPBIX[i].Key} with Port: {sortedPorts[i]}");
                }
                else 
                {
                    matchedProjects.Add(new OpenProject
                    {
                        ProjectName = "Undefined",
                        FilePath = "Undefined",
                        Port = sortedPorts[i],
                        WorkspacePath = $"Workspace_{sortedPorts[i]}", // You may need to correctly set this value
                        LastModified = DateTime.Now
                    });

                    Logger.Log($"Matched PBIX: Undefined with Port: {sortedPorts[i]}");
                }
                
            }

            return matchedProjects;
        }
    }
}
