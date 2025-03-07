using System;
using System.IO;

namespace PowerBIPlugin
{
    public static class Logger
    {
        // Change GetLogPath to static so it can be used in a static class
        private static string GetLogPath()
        {
            string basePath = AppDomain.CurrentDomain.BaseDirectory; // Get app's base directory
            string relativePath = Path.Combine(basePath, "support", "logs.txt");

            return relativePath;
        }

        public static void Log(string message)
        {
            try
            {
                // Append the log message to the log file
                using (StreamWriter writer = new StreamWriter(GetLogPath(), true))
                {
                    writer.WriteLine($"{DateTime.Now}: {message}");
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions that occur during logging
                Console.WriteLine($"Logging error: {ex.Message}");
            }
        }

        public static string GetLogs()
        {
            try
            {
                // Read all log messages from the log file
                return File.ReadAllText(GetLogPath());
            }
            catch (Exception ex)
            {
                return $"Error reading log file: {ex.Message}";
            }
        }
    }
}
