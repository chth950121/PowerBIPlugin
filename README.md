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

## Structure of this project

PowerBIOptimizer/
│
├── PowerBIOptimizer.sln                // Solution file
│
├── PowerBIOptimizer/                    // Main project folder
│   ├── Properties/                      // Project properties
│   ├── App.xaml                         // Application definition
│   ├── App.xaml.cs                      // Application code-behind
│   ├── MainWindow.xaml                  // Main window UI
│   ├── MainWindow.xaml.cs               // Main window code-behind
│   ├── Resources/                       // Resources folder
│   │   ├── Styles/                      // XAML styles
│   │   │   └── DefaultStyles.xaml       // Default styles
│   │   └── Images/                      // Images for UI
│   ├── UI/                              // UI components
│   │   ├── Controls/                    // Custom controls
│   │   │   ├── QueryListControl.xaml    // Control for displaying queries
│   │   │   └── QueryListControl.xaml.cs // Control code-behind
│   │   └── Windows/                     // Windows for dialogs
│   │       ├── OptimizedQueryDialog.xaml // Dialog for optimized queries
│   │       └── OptimizedQueryDialog.xaml.cs // Dialog code-behind
│   ├── Services/                        // Services for business logic
│   │   ├── PowerBIScanner.cs            // Service for detecting Power BI projects
│   │   ├── Optimizer.cs                 // Service for optimizing queries
│   │   └── QueryService.cs              // Service for handling queries and measures
│   ├── Models/                          // Data models
│   │   ├── Query.cs                     // Model for queries
│   │   ├── Measure.cs                   // Model for measures
│   │   ├── Column.cs                    // Model for columns
│   │   ├── Table.cs                     // Model for tables
│   │   └── DataSource.cs                // Model for data sources
│   └── Resources/                       // Additional resources
│       └── Config.json                  // Configuration file for settings
│
└── README.md                            // Project documentation

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