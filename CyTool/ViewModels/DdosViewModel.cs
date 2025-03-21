using CyTool.Commands;
using CyTool.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CyTool.ViewModels
{
    public class DdosViewModel : INotifyPropertyChanged
    {
        private readonly RequestModel _model;
        public ICommand SubmitDdos { get; }


        public DdosViewModel()
        {
            _model = new RequestModel();
            SubmitDdos = new RelayCommand(async () => await ExecuteDdosAttack());


        }

        public string TargetUrl
        {
            get => _model.TargetUrl;
            set { _model.TargetUrl = value; OnPropertyChanged(nameof(TargetUrl)); }
        }
        public int Count
        {
            get => _model.Count;
            set { _model.Count = value; OnPropertyChanged(nameof(Count)); }
        }
        public ObservableCollection<string> RequestLogs => _model.RequestLogs;


        public ObservableCollection<NetworkDevice> Devices { get; set; }
        public ICommand SendingConfiguration { get; }




        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));



        private async Task ExecuteDdosAttack()
        {
            RequestLogs.Clear();
            RequestLogs.Add($"DDOS started on {TargetUrl} with {Count} requests.");


            var listTasks = _model.PrepareAttack();
            await _model.StartDdosAttack(listTasks);

            RequestLogs.Add("DDOS attack completed.");

        }

    }
}
