using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;
using CyTool.Commands;
using CyTool.Models;

namespace CyTool.ViewModels
{
    public class DataLeakCheckerViewModel : INotifyPropertyChanged
    {
        private const string BaseUrl = "https://free.intelx.io/";
        private readonly string _apiKey;

        public DataLeakCheckerViewModel(string apiKey)
        {
            _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
            CheckCommand = new RelayCommand(
                async () => await CheckLeaksAsync(),
                () => !IsBusy && !string.IsNullOrWhiteSpace(Email)
            );
        }

        private string _email;
        public string Email
        {
            get => _email;
            set
            {
                if (_email == value) return;
                _email = value;
                OnPropertyChanged(nameof(Email));
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public ObservableCollection<DataLeakModel> Breaches { get; }
            = new ObservableCollection<DataLeakModel>();

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                if (_isBusy == value) return;
                _isBusy = value;
                OnPropertyChanged(nameof(IsBusy));
            }
        }

        private string _statusMessage;
        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                if (_statusMessage == value) return;
                _statusMessage = value;
                OnPropertyChanged(nameof(StatusMessage));
            }
        }

        public ICommand CheckCommand { get; }

        private async Task CheckLeaksAsync()
        {
            IsBusy = true;
            StatusMessage = "Searching Intelligence X…";
            Breaches.Clear();

            try
            {
                using var client = new HttpClient { BaseAddress = new Uri(BaseUrl) };
                client.DefaultRequestHeaders.UserAgent.ParseAdd("CyTool-IntelX-Client/1.0");
                client.DefaultRequestHeaders.Add("x-key", _apiKey);
                client.DefaultRequestHeaders.Accept
                      .Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var payload = new
                {
                    term = Email,
                    buckets = Array.Empty<string>(),
                    maxresults = 100,
                    lookuplevel = 0,
                    timeout = 0,
                    datefrom = "",
                    dateto = "",
                    sort = 2,
                    media = 0
                };
                var post = new StringContent(
                    JsonSerializer.Serialize(payload),
                    Encoding.UTF8,
                    "application/json"
                );

                var submitResp = await client.PostAsync("intelligent/search", post);
                if (submitResp.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    StatusMessage = "401 Unauthorized – check your API key.";
                    return;
                }
                submitResp.EnsureSuccessStatusCode();

                using var doc = JsonDocument.Parse(
                    await submitResp.Content.ReadAsStringAsync()
                );
                var searchId = doc.RootElement.GetProperty("id").GetString();

                int status;
                JsonElement records;

                do
                {
                    await Task.Delay(500);

                    var resultResp = await client.GetAsync(
                        $"intelligent/search/result?id={searchId}&offset=0&limit=100"
                    );
                    resultResp.EnsureSuccessStatusCode();

                    using var resultDoc = JsonDocument.Parse(
                        await resultResp.Content.ReadAsStringAsync()
                    );
                    status = resultDoc.RootElement.GetProperty("status").GetInt32();
                    records = resultDoc.RootElement
                                       .GetProperty("records")
                                       .Clone();
                }
                while (status == 3);

                if (records.ValueKind == JsonValueKind.Array
                    && records.GetArrayLength() > 0)
                {
                    foreach (var item in records.EnumerateArray())
                    {
                        var leak = JsonSerializer.Deserialize<DataLeakModel>(
                            item.GetRawText(),
                            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                        );
                        Breaches.Add(leak);
                    }
                    StatusMessage = $"{Breaches.Count} record(s) found.";
                }
                else
                {
                    StatusMessage = "No records found.";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
