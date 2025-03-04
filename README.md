# Power BI Optimizer Plugin

A C# plugin designed to optimize Power BI files, including DAX measures, SQL queries, and M queries.

## Features

- Detects all open Power BI files
- Identifies tables and measures
- Optimizes DAX code
- Generates optimized SQL queries
- Develops optimized M queries
- Removes unused columns

## Requirements

- .NET 9.0
- Power BI Desktop
- Visual Studio 2022 or later

## Project Structure

PowerBIPlugin/
│
├── PowerBIPlugin.sln                # Solution file
│
├── src/                              # Source code folder
│   ├── PowerBIPlugin.Core/           # Core functionality module
│   │   ├── ProjectDetector.cs        # Class for detecting Power BI projects
│   │   ├── Models/                   # Data models (if needed)
│   │   └── Services/                 # Business logic services (if needed)
│   │
│   ├── PowerBIPlugin.UI/             # User Interface module
│   │   ├── PowerBIPlugin.UI.csproj   # Project file for the UI
│   │   ├── MainWindow.xaml           # XAML file for the main window (WPF)
│   │   ├── MainWindow.xaml.cs        # Code-behind for the main window (WPF)
│   │   └── App.xaml                  # Application definition (WPF)
│   │
│   └── PowerBIPlugin.Tests/          # Unit tests module
│       ├── PowerBIPlugin.Tests.csproj # Project file for tests
│       └── ProjectDetectorTests.cs   # Unit tests for ProjectDetector
│
└── README.md                         # Documentation for the project

## Installation

1. Clone the repository or download the source code.
2. Open the solution in Visual Studio.
3. Build the solution using the command:
   ```bash
   dotnet build
   ```
4. Copy the built DLL to your Power BI Custom Visuals folder.
5. Restart Power BI Desktop.

## Usage

Here’s a quick example of how to use the plugin:

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
- Optimizes `CALCULATE` statements
- Replaces inefficient patterns

### SQL Optimization
- Removes unnecessary joins
- Adds appropriate indexes
- Optimizes `WHERE` clauses

### M Query Optimization
- Removes unnecessary transformations
- Optimizes data type conversions
- Adds error handling

## License

This project is licensed under the MIT License. See the LICENSE file for details.