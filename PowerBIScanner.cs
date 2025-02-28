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
                var schema = connection.GetSchema("TABLES");
                
                foreach (System.Data.DataRow row in schema.Rows)
                {
                    var table = new Table
                    {
                        Name = row["TABLE_NAME"].ToString() ?? "",
                        Columns = GetTableColumns(connection, row["TABLE_NAME"].ToString() ?? ""),
                        Source = DetermineTableSource(connection, row["TABLE_NAME"].ToString() ?? "")
                    };
                    tables.Add(table);
                }
            }
            return tables;
        }

        private List<Column> GetTableColumns(AdomdConnection connection, string tableName)
        {
            var columns = new List<Column>();
            var schema = connection.GetSchema("COLUMNS", new[] { tableName });
            
            foreach (System.Data.DataRow row in schema.Rows)
            {
                columns.Add(new Column
                {
                    Name = row["COLUMN_NAME"].ToString() ?? "",
                    DataType = row["DATA_TYPE"].ToString() ?? "",
                    IsUsed = CheckColumnUsage(connection, tableName, row["COLUMN_NAME"].ToString() ?? "")
                });
            }
            return columns;
        }

        private List<Measure> ScanMeasures(string filePath)
        {
            var measures = new List<Measure>();
            using (var connection = new AdomdConnection($"Data Source={filePath}"))
            {
                connection.Open();
                var schema = connection.GetSchema("MEASURES");
                
                foreach (System.Data.DataRow row in schema.Rows)
                {
                    measures.Add(new Measure
                    {
                        Name = row["MEASURE_NAME"].ToString() ?? "",
                        Expression = row["MEASURE_EXPRESSION"].ToString() ?? "",
                        TableName = row["MEASURE_TABLE"].ToString() ?? ""
                    });
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