using System.Net;
using System.Net.NetworkInformation;

namespace CyTool.Models
{
    public class NetworkDevice
    {
        public IPAddress IPAddress { get; set; }
        public Ping Ping { get; set; }
        public PingReply PingReply { get; set; }
        public IPHostEntry IPHostEntry { get; set; }
        public string name { get; set; }

        public string Status { get; set; }
    }
}