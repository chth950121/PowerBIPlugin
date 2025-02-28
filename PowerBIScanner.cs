using Microsoft.AnalysisServices.AdomdClient;
using System.Diagnostics;
using System.IO;

namespace PowerBIOptimizer
{
    public class PowerBIScanner
    {
        public List<PowerBIFile> DetectOpenFiles()
        {
            var files = new List<PowerBIFile>();
            var processes = Process.GetProcessesByName("PBIDesktop");

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
            catch
            {
                return string.Empty;
            }
        }

        private List<Table> ScanTables(string filePath)
        {
            var tables = new List<Table>();
            using (var connection = new AdomdConnection($"Data Source={filePath}"))
            {
                connection.Open();
                var cmd = connection.CreateCommand();
                cmd.CommandText = "SELECT [TABLE_NAME] FROM $System.DISCOVER_CSDL_METADATA WHERE [TABLE_TYPE] = 'TABLE'";
                
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var tableName = reader["TABLE_NAME"].ToString() ?? "";
                        var table = new Table
                        {
                            Name = tableName,
                            Columns = GetTableColumns(connection, tableName),
                            Source = DetermineTableSource(connection, tableName)
                        };
                        tables.Add(table);
                    }
                }
            }
            return tables;
        }

        private List<Column> GetTableColumns(AdomdConnection connection, string tableName)
        {
            var columns = new List<Column>();
            var cmd = connection.CreateCommand();
            cmd.CommandText = $@"
                SELECT COLUMN_NAME, DATA_TYPE 
                FROM $System.DISCOVER_CSDL_METADATA 
                WHERE TABLE_NAME = '{tableName}'";

            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    columns.Add(new Column
                    {
                        Name = reader["COLUMN_NAME"].ToString() ?? "",
                        DataType = reader["DATA_TYPE"].ToString() ?? "",
                        IsUsed = CheckColumnUsage(connection, tableName, reader["COLUMN_NAME"].ToString() ?? "")
                    });
                }
            }
            return columns;
        }

        private List<Measure> ScanMeasures(string filePath)
        {
            var measures = new List<Measure>();
            using (var connection = new AdomdConnection($"Data Source={filePath}"))
            {
                connection.Open();
                var cmd = connection.CreateCommand();
                cmd.CommandText = "SELECT [MEASURE_NAME], [MEASURE_EXPRESSION], [MEASURE_TABLE] FROM $System.DISCOVER_CSDL_METADATA WHERE [TABLE_TYPE] = 'MEASURE'";
                
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        measures.Add(new Measure
                        {
                            Name = reader["MEASURE_NAME"].ToString() ?? "",
                            Expression = reader["MEASURE_EXPRESSION"].ToString() ?? "",
                            TableName = reader["MEASURE_TABLE"].ToString() ?? ""
                        });
                    }
                }
            }
            return measures;
        }

        private DataSource DetermineTableSource(AdomdConnection connection, string tableName)
        {
            // Query DMV to get source information
            var query = $"SELECT * FROM $System.TMSCHEMA_DATA_SOURCES WHERE TableID = '{tableName}'";
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = query;
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new DataSource
                        {
                            Type = reader["Type"].ToString() ?? "",
                            ConnectionString = reader["ConnectionString"].ToString() ?? ""
                        };
                    }
                }
            }
            return new DataSource();
        }

        private bool CheckColumnUsage(AdomdConnection connection, string tableName, string columnName)
        {
            // Query DMV to check column usage in measures and relationships
            var query = $@"
                SELECT COUNT(*) 
                FROM $System.TMSCHEMA_RELATIONSHIPS 
                WHERE (FromTable = '{tableName}' AND FromColumn = '{columnName}')
                   OR (ToTable = '{tableName}' AND ToColumn = '{columnName}')";
            
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = query;
                var usageCount = (int)cmd.ExecuteScalar();
                return usageCount > 0;
            }
        }
    }
}