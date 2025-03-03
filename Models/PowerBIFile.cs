namespace PowerBIOptimizer.Models
{
    public class PowerBIFile
    {
        public string FilePath { get; set; } = string.Empty;
        public int ProcessId { get; set; }
        public List<Table> Tables { get; set; } = new List<Table>();
        public List<Measure> Measures { get; set; } = new List<Measure>();
    }
}