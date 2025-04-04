using CyTool.Commands;
using CyTool.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace CyTool.ViewModels
{
    internal class PasswordFinderViewModel : INotifyPropertyChanged
    {
        PasswordFinderModel _passwordFinderModel = new PasswordFinderModel();

        public PasswordFinderViewModel()
        {
            Passwords = new ObservableCollection<string>();
            GeneratePasswordCommand = new RelayCommand(GeneratePassword);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public ObservableCollection<string> Passwords { get; set; }
        public ICommand GeneratePasswordCommand { get; }


        public string PasswordLabel
        {
            get => _passwordFinderModel.Password;
            private set
            {
                _passwordFinderModel.Password = value;
                OnPropertyChanged(nameof(PasswordLabel));
                Clipboard.SetText(_passwordFinderModel.Password);
            }
        }

        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private void GeneratePassword()
        {
            PasswordFinderModel passwordFinder = new PasswordFinderModel();
            PasswordLabel = passwordFinder.GeneratePassword();
            

        }
    }
}
