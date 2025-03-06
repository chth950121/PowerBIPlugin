using System;
using System.Diagnostics;
using System.Security.Principal;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using PowerBIPlugin; // This should be present at the top of your file

namespace PowerBIPlugin.UI;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private QueryOptimizer queryOptimizer;
    private List<string> queries = new List<string>();

    public MainWindow()
    {
        InitializeComponent(); // Initialize the UI components first

        // Initialize projectDetector before checking for admin privileges
        queryOptimizer = new QueryOptimizer();

        if (!IsRunAsAdmin())
        {
            // Relaunch as admin
            var exePath = Process.GetCurrentProcess().MainModule.FileName;
            ProcessStartInfo psi = new ProcessStartInfo(exePath)
            {
                UseShellExecute = true,
                Verb = "runas" // Request Admin Privileges
            };
            Process.Start(psi);
            Application.Current.Shutdown(); // Close the current instance
            return; // Exit the constructor
        }
    }

    private void btnGetOpenProject_Click(object sender, RoutedEventArgs e)
    {
        List<string> ports = ProjectDetector.GetPorts();
        List<string> openPBIXFiles = ProjectDetector.GetPBIXFiles();
        List<OpenProject> openProjects = ProjectDetector.GetMatchedPP(openPBIXFiles, ports);
        
        lbOpenProjects.Items.Clear(); // Clear previous items

        if (openProjects.Count > 0)
        {
            foreach (var project in openProjects)
            {
                lbOpenProjects.Items.Add($"{project.Port} : {project.ProjectName}"); // Add each project to the ListBox
            }
        }
        else
        {
            MessageBox.Show("No open PBIX projects found.");
        }
    }

    private void lbOpenProjects_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (lbOpenProjects.SelectedItem != null)
        {
            string selectedProject = lbOpenProjects.SelectedItem.ToString();
            // Perform actions with the selected project
            MessageBox.Show($"Selected Open Project: {selectedProject}");
        }
    }

    // private void lbQueries_SelectionChanged(object sender, SelectionChangedEventArgs e)
    // {
    //     if (lbQueries.SelectedItem != null)
    //     {
    //         string selectedQuery = lbQueries.SelectedItem.ToString();
    //         // Perform actions with the selected query
    //         MessageBox.Show($"Selected Query: {selectedQuery}");
    //     }
    // }

    // private void btnGetMeasures_Click(object sender, RoutedEventArgs e)
    // {
    //     var measures = queryOptimizer.GetMeasures();
    //     lbMeasures.Items.Clear(); // Clear previous items

    //     if (measures.Count > 0)
    //     {
    //         foreach (var measure in measures)
    //         {
    //             lbMeasures.Items.Add(measure); // Add each measure to the ListBox
    //         }
    //     }
    //     else
    //     {
    //         MessageBox.Show("No measures found.");
    //     }
    // }

    static bool IsRunAsAdmin()
    {
        WindowsIdentity id = WindowsIdentity.GetCurrent();
        WindowsPrincipal principal = new WindowsPrincipal(id);
        return principal.IsInRole(WindowsBuiltInRole.Administrator);
    }
}