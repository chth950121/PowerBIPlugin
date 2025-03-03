using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using PowerBIOptimizer.Models;
using PowerBIOptimizer.Services;
using System.Collections.Generic;
using System.Windows.Input;
using PowerBIOptimizer.UI.Controls;  // Add this for QueryListControl

namespace PowerBIOptimizer.UI.Windows
{
    public partial class MainWindow : Window
    {
        private readonly PowerBIScanner _scanner;
        private readonly Optimizer _optimizer;
        private List<PowerBIFile> _currentFiles;
        private List<Measure> _allMeasures = new();
        private List<Query> _allQueries = new();

        // Make sure these match your XAML
        private QueryListControl _queryList;
        private QueryListControl _measureList;

        public MainWindow()
        {
            InitializeComponent();
            _scanner = new PowerBIScanner();
            _optimizer = new Optimizer();
            _currentFiles = new List<PowerBIFile>();
            _queryList = (QueryListControl)FindName("QueryListControl");
            _measureList = (QueryListControl)FindName("MeasureListControl");

            LoadData();
        }

        private async void LoadData()
        {
            try
            {
                StatusText.Text = "Scanning for Power BI files...";
                FooterText.Text = "Scanning...";

                // Scan for files
                _currentFiles = await Task.Run(() => _scanner.DetectOpenFiles());
                
                // Process queries and measures
                ProcessFilesData();

                // Update UI
                UpdateFileCount();
                StatusText.Text = "Ready";
                FooterText.Text = "Scan completed successfully";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                StatusText.Text = "Error scanning files";
                FooterText.Text = "Error occurred during scan";
            }
        }

        private void ProcessFilesData()
        {
            var queries = new List<object>();
            var measures = new List<object>();

            foreach (var file in _currentFiles)
            {
                // Process queries
                queries.AddRange(file.Tables.Select(t => new
                {
                    Name = t.Name,
                    Type = DetermineQueryType(t.Source.Type),
                    TableName = t.Source.Type,
                    Query = t.Source.ConnectionString,
                    FilePath = file.FilePath
                }));

                // Process measures
                measures.AddRange(file.Measures.Select(m => new
                {
                    Name = m.Name,
                    Type = "DAX",
                    TableName = m.TableName,
                    Expression = m.Expression,
                    FilePath = file.FilePath
                }));
            }

            _allQueries = queries.Cast<Query>().ToList();
            _allMeasures = measures.Cast<Measure>().ToList();

            // Update UI
            RefreshData();
        }

        private string DetermineQueryType(string sourceType)
        {
            return sourceType.ToLower() switch
            {
                var t when t.Contains("sql") => "SQL",
                var t when t.Contains("oracle") => "SQL",
                var t when t.Contains("excel") => "M Query",
                var t when t.Contains("csv") => "M Query",
                _ => "M Query"
            };
        }

        private void UpdateFileCount()
        {
            FileCountText.Text = _currentFiles.Count.ToString();
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                var searchText = textBox.Text.ToLower();
                
                if (textBox.Name == "QuerySearchBox")
                {
                    var filteredQueries = _allQueries.Where(q => 
                        q.Name.ToLower().Contains(searchText) || 
                        q.TableName.ToLower().Contains(searchText));
                    _queryList?.SetItems(filteredQueries.ToList());
                }
                else if (textBox.Name == "MeasureSearchBox")
                {
                    var filteredMeasures = _allMeasures.Where(m => 
                        m.Name.ToLower().Contains(searchText) || 
                        m.TableName.ToLower().Contains(searchText));
                    _measureList?.SetItems(filteredMeasures.ToList());
                }
            }
        }

        private void QueriesList_ItemSelected(object sender, object item)
        {
            if (item != null)
            {
                dynamic query = item;
                FooterText.Text = $"Selected query: {query.Name}";
            }
        }

        private void QueriesList_OptimizeRequested(object sender, object item)
        {
            if (item != null)
            {
                dynamic query = item;
                var dialog = new OptimizedQueryDialog(query.Name, query.Query);
                dialog.Owner = this;
                dialog.ShowDialog();
            }
        }

        private void QueriesList_ViewDetailsRequested(object sender, object item)
        {
            if (item != null)
            {
                dynamic query = item;
                MessageBox.Show(
                    $"Query Details:\n\nName: {query.Name}\nType: {query.Type}\nTable: {query.TableName}\nFile: {query.FilePath}",
                    "Query Details",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
            }
        }

        private void QueriesList_CopyRequested(object sender, object item)
        {
            if (item != null)
            {
                try
                {
                    dynamic query = item;
                    Clipboard.SetText(query.Query);
                    FooterText.Text = "Query copied to clipboard";
                }
                catch (Exception ex)
                {
                    FooterText.Text = $"Error copying to clipboard: {ex.Message}";
                }
            }
        }

        private void MeasuresList_ItemSelected(object sender, object item)
        {
            if (item != null)
            {
                dynamic measure = item;
                FooterText.Text = $"Selected measure: {measure.Name}";
            }
        }

        private void MeasuresList_OptimizeRequested(object sender, object item)
        {
            if (item != null)
            {
                dynamic measure = item;
                var dialog = new OptimizedQueryDialog(measure.Name, measure.Expression);
                dialog.Owner = this;
                dialog.ShowDialog();
            }
        }

        private void MeasuresList_ViewDetailsRequested(object sender, object item)
        {
            if (item != null)
            {
                dynamic measure = item;
                MessageBox.Show(
                    $"Measure Details:\n\nName: {measure.Name}\nTable: {measure.TableName}\nFile: {measure.FilePath}",
                    "Measure Details",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
            }
        }

        private void MeasuresList_CopyRequested(object sender, object item)
        {
            if (item != null)
            {
                try
                {
                    dynamic measure = item;
                    Clipboard.SetText(measure.Expression);
                    FooterText.Text = "Measure expression copied to clipboard";
                }
                catch (Exception ex)
                {
                    FooterText.Text = $"Error copying to clipboard: {ex.Message}";
                }
            }
        }

        private void RefreshData()
        {
            _queryList?.SetItems(_allQueries);
            _measureList?.SetItems(_allMeasures);
        }

        public void UpdateLists(List<Query> queries, List<Measure> measures)
        {
            _allQueries = queries;
            _allMeasures = measures;
            RefreshData();
        }
    }
}