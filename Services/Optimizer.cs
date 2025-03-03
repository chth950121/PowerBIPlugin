using System.Text.RegularExpressions;

namespace PowerBIOptimizer.Services
{
    public class Optimizer
    {
        public string OptimizeDax(string query)
        {
            var optimized = query;

            // Remove unnecessary variables
            optimized = RemoveUnusedVariables(optimized);

            // Optimize CALCULATE statements
            optimized = OptimizeCalculate(optimized);

            // Replace inefficient patterns
            optimized = ReplaceInefficientPatterns(optimized);

            return optimized;
        }

        public string OptimizeSql(string query)
        {
            var optimized = query;

            // Remove unnecessary joins
            optimized = RemoveUnnecessaryJoins(optimized);

            // Add appropriate indexes
            optimized = AddIndexHints(optimized);

            // Optimize WHERE clauses
            optimized = OptimizeWhereClause(optimized);

            return optimized;
        }

        public string OptimizeMQuery(string query)
        {
            var optimized = query;

            // Remove unnecessary transformations
            optimized = RemoveUnnecessarySteps(optimized);

            // Optimize data type conversions
            optimized = OptimizeDataTypes(optimized);

            // Add error handling
            optimized = AddErrorHandling(optimized);

            return optimized;
        }

        private string RemoveUnusedVariables(string dax)
        {
            var variablePattern = @"VAR\s+(\w+)\s*=";
            var variables = Regex.Matches(dax, variablePattern)
                                .Cast<Match>()
                                .Select(m => m.Groups[1].Value);

            foreach (var variable in variables)
            {
                // Check if variable is used in the RETURN statement
                var returnPattern = @"RETURN.*?" + variable;
                if (!Regex.IsMatch(dax, returnPattern))
                {
                    // Remove unused variable declaration
                    dax = Regex.Replace(dax, $@"VAR\s+{variable}\s*=.*?(?=(VAR|RETURN))", "");
                }
            }

            return dax;
        }

        private string OptimizeCalculate(string dax)
        {
            // Replace CALCULATE(SUM()) with SUMX where appropriate
            dax = Regex.Replace(
                dax,
                @"CALCULATE\s*\(\s*SUM\s*\(\s*([^)]+)\s*\)\s*,\s*([^)]+)\s*\)",
                "SUMX($2, $1)"
            );

            return dax;
        }

        private string ReplaceInefficientPatterns(string dax)
        {
            // Replace FILTER(ALL()) with VALUES() where possible
            dax = Regex.Replace(
                dax,
                @"FILTER\s*\(\s*ALL\s*\(\s*([^)]+)\s*\)\s*,\s*([^)]+)\s*\)",
                "VALUES($1)"
            );

            return dax;
        }

        private string RemoveUnnecessaryJoins(string sql)
        {
            // Analyze query structure
            var joinPattern = @"(\w+)\s+JOIN\s+(\w+)\s+ON\s+([^)]+)";
            var joins = Regex.Matches(sql, joinPattern);

            foreach (Match join in joins)
            {
                // Check if joined table's columns are used in SELECT or WHERE
                if (!IsTableUsed(sql, join.Groups[2].Value))
                {
                    // Remove unnecessary join
                    sql = Regex.Replace(sql, join.Value + @"\s*", "");
                }
            }

            return sql;
        }

        private bool IsTableUsed(string sql, string tableName)
        {
            var selectPattern = $@"SELECT.*?{tableName}\.";
            var wherePattern = $@"WHERE.*?{tableName}\.";
            return Regex.IsMatch(sql, selectPattern) || Regex.IsMatch(sql, wherePattern);
        }

        private string AddIndexHints(string sql)
        {
            // Add INDEX hints for frequently filtered columns
            var wherePattern = @"WHERE\s+(\w+\.\w+)\s*=";
            var filters = Regex.Matches(sql, wherePattern);

            foreach (Match filter in filters)
            {
                var column = filter.Groups[1].Value;
                sql = sql.Replace(
                    column,
                    $"INDEX({column})"
                );
            }

            return sql;
        }

        private string OptimizeWhereClause(string sql)
        {
            // Reorder WHERE conditions for optimal performance
            var wherePattern = @"WHERE\s+(.+)";
            var match = Regex.Match(sql, wherePattern);

            if (match.Success)
            {
                var conditions = match.Groups[1].Value.Split("AND");
                var optimizedConditions = conditions
                    .OrderBy(c => GetConditionPriority(c))
                    .ToList();

                sql = sql.Replace(
                    match.Value,
                    "WHERE " + string.Join(" AND ", optimizedConditions)
                );
            }

            return sql;
        }

        private int GetConditionPriority(string condition)
        {
            // Prioritize equality conditions over ranges
            if (condition.Contains("=")) return 1;
            if (condition.Contains("IN")) return 2;
            if (condition.Contains(">") || condition.Contains("<")) return 3;
            if (condition.Contains("LIKE")) return 4;
            return 5;
        }

        private string RemoveUnnecessarySteps(string mQuery)
        {
            // Remove redundant type transformations
            mQuery = Regex.Replace(
                mQuery,
                @"Text\.From\(Number\.From\(([^)]+)\)\)",
                "$1"
            );

            return mQuery;
        }

        private string OptimizeDataTypes(string mQuery)
        {
            // Add explicit data type conversions where needed
            var columnPattern = @"([^=]+)=\s*([^,]+)";
            var columns = Regex.Matches(mQuery, columnPattern);

            foreach (Match column in columns)
            {
                var value = column.Groups[2].Value;
                if (IsNumeric(value))
                {
                    mQuery = mQuery.Replace(
                        value,
                        $"Number.From({value})"
                    );
                }
            }

            return mQuery;
        }

        private bool IsNumeric(string value)
        {
            return double.TryParse(value, out _);
        }

        private string AddErrorHandling(string mQuery)
        {
            // Add error handling for common operations
            return $@"
try
    {mQuery}
otherwise
    error [
        Reason = ""Query failed to execute"",
        Message = Error[Message],
        Detail = Error[Detail]
    ]
";
        }
    }
}