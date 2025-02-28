# Power BI Optimizer Plugin

A C# plugin for optimizing Power BI files, including DAX measures, SQL queries, and M queries.

## Features

- Detects all open Power BI files
- Identifies tables and measures
- Optimizes DAX code
- Generates optimized SQL queries
- Develops optimized M queries
- Removes unused columns

## Requirements

- .NET 7.0
- Power BI Desktop
- Visual Studio 2022 or later

## Installation

1. Build the solution
2. Copy the built DLL to your Power BI Custom Visuals folder
3. Restart Power BI Desktop

## Usage

```csharp
var scanner = new PowerBIScanner();
var optimizer = new Optimizer();

// Detect open files
var files = scanner.DetectOpenFiles();

// Optimize a DAX measure
var optimizedDax = optimizer.OptimizeDax(measure.Expression);

// Optimize SQL query
var optimizedSql = optimizer.OptimizeSqlQuery(query);

// Optimize M query
var optimizedM = optimizer.OptimizeMQuery(mQuery);
```

## Optimization Features

### DAX Optimization
- Removes unused variables
- Optimizes CALCULATE statements
- Replaces inefficient patterns

### SQL Optimization
- Removes unnecessary joins
- Adds appropriate indexes
- Optimizes WHERE clauses

### M Query Optimization
- Removes unnecessary transformations
- Optimizes data type conversions
- Adds error handling

## License

MIT License