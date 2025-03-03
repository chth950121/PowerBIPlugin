namespace PowerBIOptimizer.Models
{
    public class DataSource
    {
        public string Type { get; set; } = string.Empty;  // Type of data source (e.g., SQL, Excel, etc.)
        public string ConnectionString { get; set; } = string.Empty;  // Connection string for the data source
    }
}
