using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using PacketDotNet;
using SharpPcap;
using SharpPcap.LibPcap;

namespace CyTool.Models
{
    public class NetworkModel
    {
        public List<NetworkDevice> Devices { get; set; } = new List<NetworkDevice>();

        public async Task StartScanNetwork(string localIPAdress)
        {
            Devices.Clear();

            string splittedIP = localIPAdress.Replace(localIPAdress.Split('.')[3], "");

            var tasks = new List<Task>();

            for (int i = 0; i < 254; i++)
            {
                int index = i;
                tasks.Add(Task.Run(async () =>
                {
                    string ip = $"{splittedIP}{index}";
                    Ping ping = new Ping();
                    PingReply reply = await ping.SendPingAsync(ip, 1000);
                    if (reply.Status == IPStatus.Success)
                    {
                        try
                        {
                            IPHostEntry host = await Dns.GetHostEntryAsync(ip);
                            string name = host.HostName;
                            Devices.Add(new NetworkDevice { IPAddress = IPAddress.Parse(ip), Ping = ping, PingReply = reply, IPHostEntry = host, name = name, Status = "Active" });
                        }
                        catch (SocketException)
                        {
                            Devices.Add(new NetworkDevice { IPAddress = IPAddress.Parse(ip), Ping = ping, PingReply = reply, IPHostEntry = null, name = "Unknown", Status = "Active" });
                        }
                    }
                }));
            }

            await Task.WhenAll(tasks);
        }

    }
}
