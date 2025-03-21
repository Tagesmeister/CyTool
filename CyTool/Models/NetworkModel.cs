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
        public List<NetworkDevice> Devices { get; private set; }
        private ICaptureDevice _captureDevice;
        private bool _capturing;

        public NetworkModel()
        {
            Devices = new List<NetworkDevice>();
        }

        #region Subnet & Device Scanning

        public string GetLocalIPAddress()
        {
            foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.OperationalStatus == OperationalStatus.Up)
                {
                    foreach (var ua in ni.GetIPProperties().UnicastAddresses)
                    {
                        if (ua.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            return ua.Address.ToString();
                        }
                    }
                }
            }
            return null;
        }

        public IPAddress GetSubnetMask()
        {
            foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.OperationalStatus == OperationalStatus.Up)
                {
                    foreach (var ua in ni.GetIPProperties().UnicastAddresses)
                    {
                        if (ua.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            return ua.IPv4Mask;
                        }
                    }
                }
            }
            return IPAddress.Parse("255.255.255.0");
        }

        public List<string> GetSubnetIPs()
        {
            List<string> ips = new List<string>();
            string localIpStr = GetLocalIPAddress();
            if (localIpStr == null)
                return ips;

            IPAddress localIp = IPAddress.Parse(localIpStr);
            IPAddress subnetMask = GetSubnetMask();

            byte[] ipBytes = localIp.GetAddressBytes();
            byte[] maskBytes = subnetMask.GetAddressBytes();

            byte[] networkBytes = new byte[ipBytes.Length];
            for (int i = 0; i < ipBytes.Length; i++)
                networkBytes[i] = (byte)(ipBytes[i] & maskBytes[i]);
            IPAddress networkAddress = new IPAddress(networkBytes);

            byte[] broadcastBytes = new byte[ipBytes.Length];
            for (int i = 0; i < ipBytes.Length; i++)
                broadcastBytes[i] = (byte)(ipBytes[i] | (~maskBytes[i]));
            IPAddress broadcastAddress = new IPAddress(broadcastBytes);

            uint network = BitConverter.ToUInt32(networkAddress.GetAddressBytes().Reverse().ToArray(), 0);
            uint broadcast = BitConverter.ToUInt32(broadcastAddress.GetAddressBytes().Reverse().ToArray(), 0);

            for (uint i = network + 1; i < broadcast; i++)
            {
                byte[] bytes = BitConverter.GetBytes(i).Reverse().ToArray();
                ips.Add(new IPAddress(bytes).ToString());
            }

            return ips;
        }


        public PhysicalAddress GetLocalMacAddress()
        {
            string localIP = GetLocalIPAddress();
            if (localIP == null)
                return null;
            foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                var ipProps = ni.GetIPProperties();
                foreach (var unicast in ipProps.UnicastAddresses)
                {
                    if (unicast.Address.ToString() == localIP)
                    {
                        return ni.GetPhysicalAddress();
                    }
                }
            }
            return null;
        }

        public async Task ScanNetworkAsync()
        {
            Devices.Clear();

            string localIP = GetLocalIPAddress();
            if (localIP == null)
            {
                Debug.WriteLine("Local IP not found.");
                return;
            }

            var localMac = GetLocalMacAddress();
            if (localMac == null)
            {
                Debug.WriteLine("Local MAC not found.");
                return;
            }

            var ips = GetSubnetIPs();
            var device = GetCaptureDevice();
            if (device == null)
            {
                Debug.WriteLine("Capture device not found.");
                return;
            }

            if (!(device is IInjectionDevice))
            {
                Debug.WriteLine("Device does not support injection. Falling back to Nmap scan.");
                await NmapScanAsync();
                return;
            }

            device.OnPacketArrival += (sender, packetCapture) =>
            {
                var rawCapture = packetCapture.GetPacket();
                var packet = PacketDotNet.Packet.ParsePacket(rawCapture.LinkLayerType, rawCapture.Data);
                var arp = packet.Extract<ArpPacket>();
                if (arp != null && arp.Operation == ArpOperation.Response)
                {
                    string senderIP = arp.SenderProtocolAddress.ToString();
                    string hostname = "N/A";
                    try { hostname = Dns.GetHostEntry(senderIP).HostName; } catch { }
                    if (!Devices.Any(d => d.IPAddress == senderIP))
                    {
                        Devices.Add(new NetworkDevice
                        {
                            IPAddress = senderIP,
                            MacAddress = arp.SenderHardwareAddress.ToString(),
                            HostName = hostname
                        });
                        Debug.WriteLine("Received ARP response from: " + senderIP);
                    }
                }
            };

            var config = new DeviceConfiguration { ReadTimeout = 1000 };
            device.Open(config);
            _capturing = true;
            device.StartCapture();
            Debug.WriteLine("Started packet capture.");

            foreach (var ip in ips)
            {
                if (ip == localIP)
                    continue;
                try
                {
                    var targetIP = IPAddress.Parse(ip);
                    var arpRequest = new ArpPacket(ArpOperation.Request,
                        PhysicalAddress.Parse("00-00-00-00-00-00"),
                        targetIP,
                        localMac,
                        IPAddress.Parse(localIP));

                    var ethernetPacket = new EthernetPacket(
                        localMac,
                        PhysicalAddress.Parse("FF-FF-FF-FF-FF-FF"),
                        EthernetType.Arp);
                    ethernetPacket.PayloadPacket = arpRequest;
                    arpRequest.ParentPacket = ethernetPacket;

                    if (device is IInjectionDevice injectionDevice)
                    {
                        injectionDevice.SendPacket(ethernetPacket);
                        Debug.WriteLine("Sent ARP request to " + ip);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Error sending ARP request to " + ip + ": " + ex.Message);
                }
            }

            await Task.Delay(5000);

            device.StopCapture();
            device.Close();
            _capturing = false;
            Debug.WriteLine("Packet capture stopped.");

            if (Devices.Count == 0)
            {
                Debug.WriteLine("No devices found via ARP. Falling back to Nmap scan.");
                await NmapScanAsync();
            }
        }

        public async Task NmapScanAsync()
        {
            string localIP = GetLocalIPAddress();
            var mask = GetSubnetMask();
            if (localIP == null || mask == null)
                return;
            var localIPAddress = IPAddress.Parse(localIP);
            byte[] ipBytes = localIPAddress.GetAddressBytes();
            byte[] maskBytes = mask.GetAddressBytes();
            for (int i = 0; i < ipBytes.Length; i++)
                ipBytes[i] = (byte)(ipBytes[i] & maskBytes[i]);
            var networkAddress = new IPAddress(ipBytes);
            int cidr = maskBytes.Sum(b => CountBits(b));
            string subnetCIDR = $"{networkAddress}/{cidr}";

            Debug.WriteLine("Running Nmap scan on subnet: " + subnetCIDR);
            string nmapOutput = await RunNmapScanAsync(subnetCIDR);
            Debug.WriteLine("Nmap output:\n" + nmapOutput);

            Regex hostRegex = new Regex(@"Nmap scan report for ([\d\.]+)", RegexOptions.Compiled);
            Regex macRegex = new Regex(@"MAC Address:\s+(([0-9A-Fa-f]{2}[:-]){5}[0-9A-Fa-f]{2})", RegexOptions.Compiled);

            var devices = new List<NetworkDevice>();
            foreach (Match match in hostRegex.Matches(nmapOutput))
            {
                string ip = match.Groups[1].Value;
                string mac = "Unknown";
                var macMatch = macRegex.Match(nmapOutput, match.Index);
                if (macMatch.Success)
                    mac = macMatch.Groups[1].Value;
                string hostname = "N/A";
                try { hostname = Dns.GetHostEntry(ip).HostName; } catch { }
                devices.Add(new NetworkDevice { IPAddress = ip, MacAddress = mac, HostName = hostname });
                Debug.WriteLine("Nmap found: " + ip + " (" + mac + ")");
            }
            Devices = devices;
        }

        private async Task<string> RunNmapScanAsync(string subnet)
        {
            var psi = new ProcessStartInfo
            {
                FileName = "nmap",
                Arguments = $"-sn {subnet}",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using (var proc = Process.Start(psi))
            {
                string output = await proc.StandardOutput.ReadToEndAsync();
                proc.WaitForExit();
                return output;
            }
        }

        private int CountBits(byte b)
        {
            int count = 0;
            while (b != 0)
            {
                count += b & 1;
                b >>= 1;
            }
            return count;
        }

        #endregion

        #region Additional Network Info

        public NetworkInfo GetLocalNetworkInfo()
        {
            var info = new NetworkInfo();
            info.LocalIPAddress = GetLocalIPAddress() ?? "N/A";
            var mask = GetSubnetMask();
            info.SubnetMask = mask?.ToString() ?? "N/A";

            if (!string.IsNullOrEmpty(info.LocalIPAddress) && mask != null && IPAddress.TryParse(info.LocalIPAddress, out IPAddress localIp))
            {
                byte[] ipBytes = localIp.GetAddressBytes();
                byte[] maskBytes = mask.GetAddressBytes();
                byte[] networkBytes = new byte[ipBytes.Length];
                for (int i = 0; i < ipBytes.Length; i++)
                    networkBytes[i] = (byte)(ipBytes[i] & maskBytes[i]);
                var networkAddress = new IPAddress(networkBytes);
                int cidr = maskBytes.Sum(b => CountBits(b));
                info.NetworkRange = $"{networkAddress}/{cidr}";
            }
            else
            {
                info.NetworkRange = "N/A";
            }
            info.HostName = Dns.GetHostName();
            info.OSVersion = Environment.OSVersion.ToString();

            long ticks = Environment.TickCount64;
            TimeSpan uptime = TimeSpan.FromMilliseconds(ticks);
            info.Uptime = uptime.ToString(@"dd\.hh\:mm\:ss");

            var adapter = NetworkInterface.GetAllNetworkInterfaces()
                            .Where(n => n.OperationalStatus == OperationalStatus.Up)
                            .OrderByDescending(n => n.Speed)
                            .FirstOrDefault();
            info.NetworkAdapter = adapter?.Description ?? "N/A";
            info.WiFiSSID = "N/A"; // Not implemented.
            return info;
        }

        #endregion

        #region Packet Capture (Other Methods)

        private ICaptureDevice GetCaptureDevice()
        {
            var devices = LibPcapLiveDeviceList.Instance;
            string localIP = GetLocalIPAddress();
            if (localIP == null)
                return null;
            foreach (var dev in devices)
            {
                if (dev.Addresses.Any(addr => addr.Addr != null &&
                                              addr.Addr.ipAddress != null &&
                                              addr.Addr.ipAddress.ToString() == localIP))
                {
                    return dev;
                }
            }
            return devices.FirstOrDefault();
        }

        public void StartPacketCapture()
        {
            _captureDevice = GetCaptureDevice();
            if (_captureDevice == null)
            {
                Console.WriteLine("No capture device found.");
                return;
            }
            _captureDevice.OnPacketArrival += Device_OnPacketArrival;
            var config = new DeviceConfiguration { ReadTimeout = 1000 };
            _captureDevice.Open(config);
            _capturing = true;
            _captureDevice.StartCapture();
            Console.WriteLine("Packet capture started.");
        }

        public void StopPacketCapture()
        {
            if (_captureDevice != null && _capturing)
            {
                _captureDevice.StopCapture();
                _captureDevice.Close();
                _capturing = false;
                Console.WriteLine("Packet capture stopped.");
            }
        }

        private void Device_OnPacketArrival(object sender, PacketCapture packetCapture)
        {
            var rawCapture = packetCapture.GetPacket();
            var packet = PacketDotNet.Packet.ParsePacket(rawCapture.LinkLayerType, rawCapture.Data);
            Console.WriteLine(packet.ToString());
        }

        #endregion
    }
}
