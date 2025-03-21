using System.Windows;
using CyTool.Views; // Ensure that your AnalyseNetworkControl is in this namespace

namespace CyTool
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        // Click event handler for the "Analyse Network" button.
        private void AnalyseNetworkButton_Click(object sender, RoutedEventArgs e)
        {
            // Debug message to ensure the event fires.

            // Create an instance of your AnalyseNetworkControl.
            var analyseNetworkControl = new AnalyseNetworkControl();

            // Create a new window to host the AnalyseNetworkControl.
            var networkWindow = new Window
            {
                Title = "Analyse Network",
                Content = analyseNetworkControl,
                Width = 900,
                Height = 600,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                Owner = this,     // Set the MainWindow as the owner.
                Topmost = true    // Optionally set this to ensure it appears on top.
            };

            // Show the new window.
            networkWindow.Show();
        }

        private void DDoSButton_Click(object sender, RoutedEventArgs e)
        {
            var DdosPage = new DdosPage();

            var ddosWindow = new Window
            {
                Title = "DDoS",
                Content = DdosPage,
                Width = 900,
                Height = 600,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                Owner = this,
                Topmost = true
            };

            ddosWindow.Show();

        }

        private void BruteForceButton_Click(object sender, RoutedEventArgs e)
        {
            var bruteForceControl = new BruteForceControl();
            var networkWindow = new Window
            {
                Title = "Brute Force",
                Content = bruteForceControl,
                Width = 900,
                Height = 600,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                Owner = this,
                Topmost = true
            };
            networkWindow.Show();
        }
    }
}