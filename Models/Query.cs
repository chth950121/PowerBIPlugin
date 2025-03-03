namespace PowerBIOptimizer.Models
{
    public class Query
    {
        public string Name { get; set; } = string.Empty;
        public string Expression { get; set; } = string.Empty;
        public string TableName { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;  // SQL, DAX, or M Query
    }
}