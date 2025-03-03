using System;
using System.Windows;
using System.Windows.Controls;
using PowerBIOptimizer.Services;

namespace PowerBIOptimizer.UI.Windows
{
    public partial class OptimizedQueryDialog : Window
    {
        private readonly Optimizer _optimizer;
        
        public OptimizedQueryDialog(string queryName, string originalQuery)
        {
            InitializeComponent();
            _optimizer = new Optimizer();
            
            Title = $"Optimize Query - {queryName}";
            QueryTextBox.Text = originalQuery;
            OptimizedQueryTextBox.Text = string.Empty;
        }

        private void QueryTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            OptimizeQuery();
        }

        private void OptimizeButton_Click(object sender, RoutedEventArgs e)
        {
            OptimizeQuery();
        }

        private void OptimizeQuery()
        {
            try
            {
                var queryType = QueryTypeComboBox.SelectedItem?.ToString() ?? "";
                var originalQuery = QueryTextBox.Text;
                var optimizedQuery = queryType switch
                {
                    "DAX" => _optimizer.OptimizeDax(originalQuery),
                    "M Query" => _optimizer.OptimizeMQuery(originalQuery),
                    "SQL" => _optimizer.OptimizeSql(originalQuery),
                    _ => originalQuery
                };
                OptimizedQueryTextBox.Text = optimizedQuery;
            }
            catch (Exception ex)
            {
                OptimizedQueryTextBox.Text = $"Error during optimization: {ex.Message}";
            }
        }

        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(QueryTextBox.Text);
            MessageBox.Show("Query copied to clipboard!", "Success", 
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
