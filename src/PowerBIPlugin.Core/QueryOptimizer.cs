using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AnalysisServices.Tabular; // Correct namespace for Power BI Tabular Model
using Microsoft.AnalysisServices.AdomdClient;

namespace PowerBIPlugin
{
    public class QueryOptimizer
    {
        public static List<string> GetQueries(string port)
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
    }
}
