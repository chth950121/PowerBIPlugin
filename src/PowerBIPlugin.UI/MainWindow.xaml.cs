using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Principal;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;  // FIX: Added missing namespace for Brushes
using System.Text.Json;

namespace PowerBIPlugin.UI
{
    public partial class MainWindow : Window
    {
        private readonly PowerBIService powerBIService;

        public MainWindow()
        {
            InitializeComponent();
            var fileSystemService = new FileSystemService();
            var processService = new ProcessService();

            powerBIService = new PowerBIService(fileSystemService, processService);

            EnsureRunAsAdmin();
        }

        // Ensures the application runs as an Administrator
        private void EnsureRunAsAdmin()
        {
            if (!IsRunAsAdmin())
            {
                string exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                ProcessStartInfo psi = new ProcessStartInfo(exePath)
                {
                    UseShellExecute = true,
                    Verb = "runas" // Request Admin Privileges
                };
                Process.Start(psi);
                Application.Current.Shutdown();
            }
        }

        private void btnGetOpenProject_Click(object sender, RoutedEventArgs e)
        {
            lbOpenProjects.Items.Clear();
            var openProjects = powerBIService.GetOpenPowerBIProjects();

            if (openProjects.Count > 0)
            {
                foreach (var project in openProjects)
                {
                    lbOpenProjects.Items.Add($"{project.Port} : {project.ProjectName}");
                }
            }
            else
            {
                MessageBox.Show("No open PBIX projects found.");
            }
        }

        private async void btnGenerateOptimizedQuery_Click(object sender, RoutedEventArgs e)
        {
            if (lbQueries.SelectedItem != null)
            {
                string selectedQuery = lbQueries.SelectedItem.ToString();
                
                // Ensure selectedQuery is not null or empty
                if (string.IsNullOrEmpty(selectedQuery))
                {
                    MessageBox.Show("Selected query is empty or null.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string response = await OpenAIService.GetResponseFromOpenAI($"Optimize this Power BI M Query: {selectedQuery}");

                // Check if response contains an error
                if (response.Contains("\"error\""))
                {
                    try
                    {
                        using JsonDocument doc = JsonDocument.Parse(response);
                        JsonElement root = doc.RootElement;

                        if (root.TryGetProperty("error", out JsonElement errorElement) &&
                            errorElement.TryGetProperty("message", out JsonElement messageElement))
                        {
                            string errorMessage = messageElement.GetString();
                            MessageBox.Show($"Error: {errorMessage}", "OpenAI API Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            return; // Exit function to prevent adding error to listbox
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"An error occurred while processing the API response: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }

                // If no error, add optimized query to the list
                lbGeneratedQueries.Items.Add(response);
            }
            else
            {
                MessageBox.Show("Please select a query to optimize.", "Selection Required", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async void btnGenerateOptimizedMeasure_Click(object sender, RoutedEventArgs e)
        {
            if (lbMeasures.SelectedItem != null)
            {
                string selectedMeasure = lbMeasures.SelectedItem.ToString();

                // Ensure selectedMeasure is not null or empty
                if (string.IsNullOrEmpty(selectedMeasure))
                {
                    MessageBox.Show("Selected measure is empty or null.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string response = await OpenAIService.GetResponseFromOpenAI($"Optimize this Power BI Measure: {selectedMeasure}");

                // Check if response contains an error
                if (response.Contains("\"error\""))
                {
                    try
                    {
                        using JsonDocument doc = JsonDocument.Parse(response);
                        JsonElement root = doc.RootElement;

                        if (root.TryGetProperty("error", out JsonElement errorElement) &&
                            errorElement.TryGetProperty("message", out JsonElement messageElement))
                        {
                            string errorMessage = messageElement.GetString();
                            MessageBox.Show($"Error: {errorMessage}", "OpenAI API Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            return; // Exit function to prevent adding error to listbox
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"An error occurred while processing the API response: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }

                // If no error, add optimized measure to the list
                lbGeneratedQueries.Items.Add(response);
            }
            else
            {
                MessageBox.Show("Please select a measure to optimize.", "Selection Required", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void lbOpenProjects_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lbOpenProjects.SelectedItem is null) return;

            lbQueries.Items.Clear();
            lbMeasures.Items.Clear();

            string selectedProject = lbOpenProjects.SelectedItem?.ToString() ?? string.Empty;
            string[] parts = selectedProject.Split(new string[] { " : " }, StringSplitOptions.None);
            if (parts.Length < 2) return;

            string port = parts[0].Trim();

            try
            {
                List<string> queries = powerBIService.GetQueries(port);
                List<string> measures = powerBIService.GetMeasures(port);

                foreach (var query in queries) lbQueries.Items.Add(query);
                foreach (var measure in measures) lbMeasures.Items.Add(measure);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error: {ex.Message}");
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        private static bool IsRunAsAdmin()
        {
            WindowsIdentity id = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(id);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
    }
}
