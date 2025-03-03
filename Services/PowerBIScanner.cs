using Microsoft.AnalysisServices.AdomdClient;
using System.Diagnostics;
using System.IO;
using System;
using System.Collections.Generic;
using PBIMeasure = PowerBIOptimizer.Models.Measure;

namespace PowerBIOptimizer.Services
{
    public class PowerBIScanner
    {
        private const string PowerBIProcessName = "PBIDesktop";
        private const string TableQueryTemplate = @"
            SELECT [TABLE_NAME] 
            FROM $System.DISCOVER_CSDL_METADATA 
            WHERE [TABLE_TYPE] = 'TABLE'";
        private const string MeasureQueryTemplate = @"
            SELECT [MEASURE_NAME], [MEASURE_EXPRESSION], [MEASURE_TABLE] 
            FROM $System.DISCOVER_CSDL_METADATA 
            WHERE [TABLE_TYPE] = 'MEASURE'";

        public List<Models.PowerBIFile> DetectOpenFiles()
        {
            var files = new List<Models.PowerBIFile>();
            var processes = Process.GetProcessesByName(PowerBIProcessName);

            foreach (var process in processes)
            {
                try
                {
                    var filePath = GetFilePathFromProcess(process);
                    if (!string.IsNullOrEmpty(filePath))
                    {
                        var file = new Models.PowerBIFile
                        {
                            FilePath = filePath,
                            ProcessId = process.Id,
                            Tables = ScanTables(filePath),
                            Measures = ScanMeasures(filePath)
                        };
                        files.Add(file);
                    }
                }
                catch (Exception ex)
                {
                    // Log error and continue with next file
                    Console.WriteLine($"Error scanning file: {ex.Message}");
                }
            }

            return files;
        }

        private string GetFilePathFromProcess(Process process)
        {
            try
            {
                return process.MainModule?.FileName ?? string.Empty;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        private List<Models.Table> ScanTables(string filePath)
        {
            var tables = new List<Models.Table>();
            using var connection = CreateConnection(filePath);
            
            using var command = connection.CreateCommand();
            command.CommandText = TableQueryTemplate;
            
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var tableName = reader["TABLE_NAME"].ToString() ?? string.Empty;
                var table = new Models.Table
                {
                    Name = tableName,
                    Columns = GetTableColumns(connection, tableName),
                    Source = DetermineTableSource(connection, tableName)
                };
                tables.Add(table);
            }

            return tables;
        }

        private List<Models.Column> GetTableColumns(AdomdConnection connection, string tableName)
        {
            var columns = new List<Models.Column>();
            using var command = connection.CreateCommand();
            command.CommandText = $@"
                SELECT COLUMN_NAME, DATA_TYPE 
                FROM $System.DISCOVER_CSDL_METADATA 
                WHERE TABLE_NAME = '{tableName}'";

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var columnName = reader["COLUMN_NAME"].ToString() ?? string.Empty;
                columns.Add(new Models.Column
                {
                    Name = columnName,
                    DataType = reader["DATA_TYPE"].ToString() ?? string.Empty,
                    IsUsed = CheckColumnUsage(connection, tableName, columnName)
                });
            }

            return columns;
        }

        private List<PBIMeasure> ScanMeasures(string filePath)
        {
            var measures = new List<PBIMeasure>();
            using var connection = CreateConnection(filePath);
            
            using var command = connection.CreateCommand();
            command.CommandText = MeasureQueryTemplate;
            
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                measures.Add(new PBIMeasure
                {
                    Name = reader["MEASURE_NAME"].ToString() ?? string.Empty,
                    Expression = reader["MEASURE_EXPRESSION"].ToString() ?? string.Empty,
                    TableName = reader["MEASURE_TABLE"].ToString() ?? string.Empty
                });
            }

            return measures;
        }

        private Models.DataSource DetermineTableSource(AdomdConnection connection, string tableName)
        {
            using var command = connection.CreateCommand();
            command.CommandText = $@"
                SELECT * 
                FROM $System.TMSCHEMA_DATA_SOURCES 
                WHERE TableID = '{tableName}'";

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return new Models.DataSource
                {
                    Type = reader["Type"].ToString() ?? string.Empty,
                    ConnectionString = reader["ConnectionString"].ToString() ?? string.Empty
                };
            }

            return new Models.DataSource();
        }

        private bool CheckColumnUsage(AdomdConnection connection, string tableName, string columnName)
        {
            using var command = connection.CreateCommand();
            command.CommandText = $@"
                SELECT COUNT(*) 
                FROM $System.TMSCHEMA_RELATIONSHIPS 
                WHERE (FromTable = '{tableName}' AND FromColumn = '{columnName}')
                   OR (ToTable = '{tableName}' AND ToColumn = '{columnName}')";

            var usageCount = Convert.ToInt32(command.ExecuteScalar());
            return usageCount > 0;
        }

        private AdomdConnection CreateConnection(string filePath)
        {
            var connection = new AdomdConnection($"Data Source={filePath}");
            try
            {
                connection.Open();
                return connection;
            }
            catch (Exception)
            {
                connection.Dispose();
                throw;
            }
        }
    }
}