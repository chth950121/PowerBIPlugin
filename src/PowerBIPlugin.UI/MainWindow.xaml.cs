using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Principal;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;  // FIX: Added missing namespace for Brushes

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
            List<string> queries = new List<string>();
            foreach (var item in lbQueries.Items)
            {
                queries.Add(item.ToString());
            }

            // Ensure that you're calling the method correctly
            string optimizedQuery = await OpenAIService.GetResponseFromOpenAI("Optimize this Power BI M Query:", queries);

            // You can now use optimizedQuery, such as displaying it or storing it
            MessageBox.Show(optimizedQuery, "Optimized Query");
        }


        private void btnGenerateOptimizedMeasure_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Generate Optimized Measure button clicked!");
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
