using System.Windows.Controls;
using CyTool.ViewModels;

namespace CyTool.Views
{
    /// <summary>
    /// Interaction logic for AnalyseNetworkControl.xaml
    /// </summary>
    public partial class AnalyseNetworkControl : UserControl
    {
        public AnalyseNetworkControl()
        {
            InitializeComponent();
            DataContext = new AnalyseNetworkViewModel();
        }
    }
}
