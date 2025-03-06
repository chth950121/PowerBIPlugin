using System;
using System.IO;

namespace PowerBIPlugin
{
    public static class Logger
    {
        private static readonly string logFilePath = @"E:\github\PowerBIPlugin\logs.txt"; // Change the path as needed

        public static void Log(string message)
        {
            try
            {
                // Append the log message to the log file
                using (StreamWriter writer = new StreamWriter(logFilePath, true))
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
                return File.ReadAllText(logFilePath);
            }
            catch (Exception ex)
            {
                return $"Error reading log file: {ex.Message}";
            }
        }
    }
}