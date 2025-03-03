using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.AnalysisServices.AdomdClient;
using PowerBIOptimizer.Models;

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

        public List<PowerBIFile> DetectOpenFiles()
        {
            var files = new List<PowerBIFile>();
            var processes = Process.GetProcessesByName(PowerBIProcessName);

            foreach (var process in processes)
            {
                try
                {
                    var filePath = GetFilePathFromProcess(process);
                    if (!string.IsNullOrEmpty(filePath))
                    {
                        var file = new PowerBIFile
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

        private List<Table> ScanTables(string filePath)
        {
            var tables = new List<Table>();
            using var connection = CreateConnection(filePath);
            
            using var command = connection.CreateCommand();
            command.CommandText = TableQueryTemplate;
            
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var tableName = reader["TABLE_NAME"].ToString() ?? string.Empty;
                var table = new Table
                {
                    Name = tableName,
                    Columns = GetTableColumns(connection, tableName),
                    Source = DetermineTableSource(connection, tableName)
                };
                tables.Add(table);
            }

            return tables;
        }

        private List<Column> GetTableColumns(AdomdConnection connection, string tableName)
        {
            var columns = new List<Column>();
            using var command = connection.CreateCommand();
            command.CommandText = $@"
                SELECT COLUMN_NAME, DATA_TYPE 
                FROM $System.DISCOVER_CSDL_METADATA 
                WHERE TABLE_NAME = '{tableName}'";

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var columnName = reader["COLUMN_NAME"].ToString() ?? string.Empty;
                columns.Add(new Column
                {
                    Name = columnName,
                    DataType = reader["DATA_TYPE"].ToString() ?? string.Empty,
                    IsUsed = CheckColumnUsage(connection, tableName, columnName)
                });
            }

            return columns;
        }

        private List<Measure> ScanMeasures(string filePath)
        {
            var measures = new List<Measure>();
            using var connection = CreateConnection(filePath);
            
            using var command = connection.CreateCommand();
            command.CommandText = MeasureQueryTemplate;
            
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                measures.Add(new Measure
                {
                    Name = reader["MEASURE_NAME"].ToString() ?? string.Empty,
                    Expression = reader["MEASURE_EXPRESSION"].ToString() ?? string.Empty,
                    TableName = reader["MEASURE_TABLE"].ToString() ?? string.Empty
                });
            }

            return measures;
        }

        private DataSource DetermineTableSource(AdomdConnection connection, string tableName)
        {
            using var command = connection.CreateCommand();
            command.CommandText = $@"
                SELECT * 
                FROM $System.TMSCHEMA_DATA_SOURCES 
                WHERE TableID = '{tableName}'";

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return new DataSource
                {
                    Type = reader["Type"].ToString() ?? string.Empty,
                    ConnectionString = reader["ConnectionString"].ToString() ?? string.Empty
                };
            }

            return new DataSource();
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

        public void ScanForPowerBIProjects()
        {
            // Implement logic to detect open Power BI projects
        }
    }
}
