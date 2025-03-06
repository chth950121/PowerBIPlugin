using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AnalysisServices.Tabular; // Correct namespace for Power BI Tabular Model

namespace PowerBIPlugin
{
    public class QueryOptimizer
    {
        public List<string> GetQueries(string port)
        {
            List<string> queries = new List<string>();
            string connectionString = $"DataSource=localhost:{port}";

            Logger.Log($"Retrieving queries from Power BI instance on port {port}.");

            using (Server server = new Server()) // Use Tabular.Server
            {
                try
                {
                    server.Connect(connectionString);

                    if (server.Databases.Count > 0)
                    {
                        Database db = server.Databases[0]; // Now correctly references a Tabular Database

                        if (db.Model != null)
                        {
                            Logger.Log($"Connected to Power BI Instance on Port: {port}");

                            foreach (var table in db.Model.Tables)
                            {
                                foreach (var partition in table.Partitions)
                                {
                                    if (partition.Source is MPartitionSource mPartition)
                                    {
                                        string query = $"Table: {table.Name}\nM Query:\n{mPartition.Expression}";
                                        Logger.Log(query);
                                        queries.Add(query);
                                    }
                                }
                            }
                        }
                        else
                        {
                            Logger.Log($"No Tabular Model found on port {port}.");
                        }
                    }
                    else
                    {
                        Logger.Log($"No databases found on port {port}.");
                    }

                    server.Disconnect();
                }
                catch (Exception ex)
                {
                    Logger.Log($"Error retrieving queries from Power BI on port {port}: {ex.Message}");
                }
            }
            return queries;
        }
    }
}
