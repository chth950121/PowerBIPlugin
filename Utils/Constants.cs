namespace PowerBIOptimizer.Utils
{
    public static class Constants
    {
        // Process related
        public const string PowerBIProcessName = "PBIDesktop";

        // Error messages
        public const string ErrorLoadingData = "Error loading data: {0}";
        public const string ErrorOptimizingQuery = "Error optimizing query: {0}";

        // Success messages
        public const string QueryCopiedMessage = "Query copied to clipboard!";

        // Dialog titles
        public const string ErrorTitle = "Error";
        public const string SuccessTitle = "Success";

        // Query types
        public const string DaxQueryType = "DAX";
        public const string SqlQueryType = "SQL";
        public const string MQueryType = "M";
    }
}
