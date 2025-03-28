using System.Windows;
using CyTool.Views;

namespace CyTool
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void AnalyseNetworkButton_Click(object sender, RoutedEventArgs e)
        {
            var analyseNetworkControl = new AnalyseNetworkControl();

            var networkWindow = new Window
            {
                Title = "Analyse Network",
                Content = analyseNetworkControl,
                Width = 900,
                Height = 600,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                Owner = this,
                Topmost = true
            };

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
            var bfWindow = new Window
            {
                Title = "Brute Force",
                Content = bruteForceControl,
                Width = 900,
                Height = 600,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                Owner = this,
                Topmost = true
            };
            bfWindow.Show();
        }
        private void DirectoryBruteforce_Click(object sender, RoutedEventArgs e)
        {
            var directoryBruteforceControl = new DirectoryBruteforceControl();
            var dbfWindow = new Window
            {
                Title = "Directory Bruteforce",
                Content = directoryBruteforceControl,
                Width = 900,
                Height = 600,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                Owner = this,
                Topmost = true
            };
            dbfWindow.Show();
        }
    }
}