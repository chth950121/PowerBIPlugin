namespace PowerBIOptimizer.Models
{
    public class Column
    {
        public string Name { get; set; } = string.Empty;
        public string DataType { get; set; } = string.Empty;
        public bool IsUsed { get; set; }
    }
}