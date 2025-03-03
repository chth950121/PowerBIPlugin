namespace PowerBIOptimizer.Models
{
    public class Table
    {
        public string Name { get; set; } = string.Empty;
        public List<Column> Columns { get; set; } = new List<Column>();
        public DataSource Source { get; set; } = new DataSource();  // Reference to the data source
    }
}