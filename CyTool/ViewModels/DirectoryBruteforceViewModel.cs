using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Windows.Input;
using Microsoft.Win32;
using CyTool.Commands;

namespace CyTool.ViewModels
{
    public class DirectoryBruteforceViewModel : INotifyPropertyChanged
    {
        private string _baseUrl;
        private string _outputLog;
        private bool _isRunning;
        private CancellationTokenSource _cts;

        public string BaseUrl
        {
            get => _baseUrl;
            set
            {
                _baseUrl = value;
                OnPropertyChanged(nameof(BaseUrl));
            }
        }

        public string OutputLog
        {
            get => _outputLog;
            set
            {
                _outputLog = value;
                OnPropertyChanged(nameof(OutputLog));
            }
        }

        public ICommand StartEnumerationCommand { get; }
        public ICommand StopEnumerationCommand { get; }
        public ICommand ExportOutputCommand { get; }

        public DirectoryBruteforceViewModel()
        {
            StartEnumerationCommand = new RelayCommand(async () => await StartEnumeration(), () => !_isRunning);
            StopEnumerationCommand = new RelayCommand(StopEnumeration, () => _isRunning);
            ExportOutputCommand = new RelayCommand(ExportOutput);
        }

        private async Task StartEnumeration()
        {
            if (string.IsNullOrWhiteSpace(BaseUrl))
            {
                OutputLog += "Please enter a valid Base URL.\n";
                return;
            }

            OutputLog = string.Empty;

            string projectDir = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)
                                         .Parent.Parent.Parent.FullName;
            string pathsFilePath = Path.Combine(projectDir, "Resources", "paths.txt");

            if (!File.Exists(pathsFilePath))
            {
                OutputLog += $"The paths file was not found: {pathsFilePath}\n";
                return;
            }

            _isRunning = true;
            _cts = new CancellationTokenSource();
            OutputLog += "Starting directory enumeration...\n";

            var handler = new HttpClientHandler { AllowAutoRedirect = false };
            using (var client = new HttpClient(handler))
            {
                try
                {
                    var paths = await File.ReadAllLinesAsync(pathsFilePath);
                    foreach (var path in paths)
                    {
                        if (_cts.IsCancellationRequested)
                            break;

                        var trimmedPath = path.Trim();
                        if (string.IsNullOrWhiteSpace(trimmedPath))
                            continue;

                        var url = $"{BaseUrl.TrimEnd('/')}{trimmedPath}";
                        try
                        {
                            var response = await client.GetAsync(url, _cts.Token);

                            if ((int)response.StatusCode >= 300 && (int)response.StatusCode < 400)
                            {
                                var location = response.Headers.Location?.ToString() ?? "";
                                if (!string.IsNullOrEmpty(location) && location.Contains(BaseUrl.TrimEnd('/')))
                                {
                                    OutputLog += $"Redirect detected at {url} -> {location}. Not a valid hit.\n";
                                    continue;
                                }
                            }

                            if (response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.Forbidden || response.StatusCode == HttpStatusCode.Unauthorized)
                            {
                                OutputLog += $"Found: {url} - Status: {response.StatusCode}\n";
                            }
                            else
                            {
                                OutputLog += $"Checked: {url} - Status: {response.StatusCode}\n";
                            }
                        }
                        catch (Exception ex)
                        {
                            OutputLog += $"Error at {url}: {ex.Message}\n";
                        }
                    }
                    OutputLog += "Enumeration complete.\n";
                }
                catch (Exception ex)
                {
                    OutputLog += $"Error: {ex.Message}\n";
                }
                finally
                {
                    _isRunning = false;
                }
            }
        }

        private void StopEnumeration()
        {
            if (_isRunning && _cts != null)
            {
                _cts.Cancel();
                OutputLog += "Enumeration stopped by user.\n";
                _isRunning = false;
            }
        }

        private void ExportOutput()
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",
                FileName = "ExportedOutput.txt"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    File.WriteAllText(saveFileDialog.FileName, OutputLog);
                    OutputLog += $"Output exported to: {saveFileDialog.FileName}\n";
                }
                catch (Exception ex)
                {
                    OutputLog += $"Export error: {ex.Message}\n";
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
