namespace CyTool.Models
{
    public class NetworkInfo
    {
        public string LocalIPAddress { get; set; }
        public string SubnetMask { get; set; }
        public string NetworkRange { get; set; }
        public string HostName { get; set; }
        public string OSVersion { get; set; }
        public string Uptime { get; set; }
        public string NetworkAdapter { get; set; }
        public string WiFiSSID { get; set; }  // Not implemented

        public override string ToString()
        {
            return $"Local IP: {LocalIPAddress}\n" +
                   $"Subnet Mask: {SubnetMask}\n" +
                   $"Network Range: {NetworkRange}\n" +
                   $"Host Name: {HostName}\n" +
                   $"OS Version: {OSVersion}\n" +
                   $"Uptime: {Uptime}\n" +
                   $"Network Adapter: {NetworkAdapter}\n" +
                   $"WiFi SSID: {WiFiSSID}";
        }
    }
}