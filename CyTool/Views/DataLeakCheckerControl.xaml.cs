using System.Windows.Controls;
using CyTool.ViewModels;

namespace CyTool.Views
{
    public partial class DataLeakCheckerControl : UserControl
    {
        public DataLeakCheckerControl()
        {
            InitializeComponent();

            var apiKey = App.Configuration["IntelX:ApiKey"];
            DataContext = new DataLeakCheckerViewModel(apiKey);
        }
    }
}