namespace PowerBIOptimizer
{
    public class PowerBIFile
    {
        public string FilePath { get; set; } = "";
        public int ProcessId { get; set; }
        public List<Table> Tables { get; set; } = new();
        public List<Measure> Measures { get; set; } = new();
    }

    public class Table
    {
        public string Name { get; set; } = "";
        public List<Column> Columns { get; set; } = new();
        public DataSource Source { get; set; } = new();
    }

    public class Column
    {
        public string Name { get; set; } = "";
        public string DataType { get; set; } = "";
        public bool IsUsed { get; set; }
    }

    public class Measure
    {
        public string Name { get; set; } = "";
        public string Expression { get; set; } = "";
        public string TableName { get; set; } = "";
    }

    public class DataSource
    {
        public string Type { get; set; } = "";
        public string ConnectionString { get; set; } = "";
    }
}