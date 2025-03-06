using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Principal;
using System.Windows;
using System.Windows.Controls;

namespace PowerBIPlugin.UI
{
<<<<<<< HEAD
    public partial class MainWindow : Window
=======
    private List<string> queries = new List<string>();

    public MainWindow()
>>>>>>> d536eee6ccee2f7a409fa6954e971757314dcf77
    {
        private readonly PowerBIService powerBIService;

<<<<<<< HEAD
        public MainWindow()
=======
        if (!IsRunAsAdmin())
>>>>>>> d536eee6ccee2f7a409fa6954e971757314dcf77
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

<<<<<<< HEAD
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

        private void lbOpenProjects_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lbOpenProjects.SelectedItem is null) return;

            lbQueries.Items.Clear();
            lbMeasures.Items.Clear();

            string selectedProject = lbOpenProjects.SelectedItem.ToString();
            string port = selectedProject.Split(new string[] { " : " }, StringSplitOptions.None)[0].Trim();

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
=======
    private void lbOpenProjects_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (lbOpenProjects.SelectedItem is null) return;

        lbQueries.Items.Clear();

        string selectedProject = lbOpenProjects.SelectedItem.ToString();
        string port = selectedProject.Split(new string[] { " : " }, StringSplitOptions.None)[0].Trim();

        try
        {
            List<string> queries = QueryOptimizer.GetQueries(port);

            foreach (var query in queries) lbQueries.Items.Add(query);
            foreach (var query in queries) Logger.Log(query);
        }
        catch (Exception ex)
        {
            Logger.Log($"Error: {ex.Message}");
            MessageBox.Show($"Error: {ex.Message}");
>>>>>>> d536eee6ccee2f7a409fa6954e971757314dcf77
        }
    }
}
