using CyTool.ViewModels;
using Microsoft.Extensions.Configuration;
using System.Configuration;
using System.Data;
using System.Windows;

namespace CyTool
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static IConfiguration Configuration { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            Configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddUserSecrets<App>()
                .Build();

            var apiKey = Configuration["IntelX:ApiKey"];
            var vm = new DataLeakCheckerViewModel(apiKey);
            var window = new MainWindow { DataContext = vm };
        }
    }

}
