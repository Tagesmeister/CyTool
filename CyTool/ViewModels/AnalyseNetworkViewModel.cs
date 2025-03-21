using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using CyTool.Commands;
using CyTool.Models;

namespace CyTool.ViewModels
{
    public class AnalyseNetworkViewModel : INotifyPropertyChanged
    {
        private string _status;
        public string Status
        {
            get => _status;
            set { _status = value; OnPropertyChanged(nameof(Status)); }
        }

        private string _additionalInfo;
        public string AdditionalInfo
        {
            get => _additionalInfo;
            set { _additionalInfo = value; OnPropertyChanged(nameof(AdditionalInfo)); }
        }

        public ObservableCollection<NetworkDevice> Devices { get; set; }
        public ICommand ScanNetworkCommand { get; }

        private NetworkModel _networkModel;

        public AnalyseNetworkViewModel()
        {
            _networkModel = new NetworkModel();
            Devices = new ObservableCollection<NetworkDevice>();
            ScanNetworkCommand = new RelayCommand(async () => await ScanNetworkAsync());
            Status = "Ready for network analysis.";

            AdditionalInfo = _networkModel.GetLocalNetworkInfo().ToString();
        }

        private async Task ScanNetworkAsync()
        {
            Status = "Scanning network...";
            await _networkModel.ScanNetworkAsync();
            Devices.Clear();
            foreach (var device in _networkModel.Devices)
                Devices.Add(device);
            Status = $"Scan complete. {Devices.Count} devices found.";
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}