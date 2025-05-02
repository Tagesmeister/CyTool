using System.Net;
using System.Net.Sockets;

namespace CyTool.Models
{
    public class NetworkInfo
    {
        public static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            return ("No network adapters with an IPv4 address in the system!");
        }

    }
}
