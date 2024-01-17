using System.Net;
using System.Net.Sockets;

namespace Tikhole.Engine
{
    public class Committer : IDisposable
    {
        public static string ListTTL = "24h";
        public static string UserName = "Tikhole";
        public static string Password = "";
        public static uint Committed = 0;
        public static uint ComitterTimeoutMS = 1000;
        public static uint ComitterDelayMS = 100;
        public static uint TotalInstances { get; set; } = 0;
        public static uint NeededInstances = 2;
        public static IPEndPoint RouterOSIPEndPoint = new(IPAddress.Parse("192.168.200.1"), 8728);
        public TcpClient TcpClient = new();
        private uint InstanceID = 0;
        private uint ResponsesRecieved = 0;
        private SemaphoreSlim Semaphore = new(1, 1);
        public Committer()
        {
            InstanceID = TotalInstances++;
            if (Tikhole.Matcher != null) Tikhole.Matcher.ResponseMatched += Matcher_ResponseMatched;
        }
        private void Login()
        {
            try
            {
                Logger.Info("Connecting to " + RouterOSIPEndPoint.ToString() + "...");
                TcpClient = new();
                TcpClient.Connect(RouterOSIPEndPoint);
                TcpClient.SendSentence([
                    "/login",
                    "=name=" + UserName,
                    "=password=" + Password
                ]);
                string[] response = TcpClient.SendSentence([
                    "/ip/firewall/address-list/print",
                    "=.proplist=.id,list,address,timeout",
                    "",
                    "/ipv6/firewall/address-list/print",
                    "=.proplist=.id,list,address,timeout"
                ]);
                Logger.Success("Connected to " + RouterOSIPEndPoint.ToString() + ".");
            }
            catch
            {
                Logger.Warning("Failed to connect to " + RouterOSIPEndPoint.ToString() + ".");
            }
        }
        private void Matcher_ResponseMatched(object? sender, ResponseMatchedEventArgs e)
        {
            if (ResponsesRecieved++ % TotalInstances != InstanceID) return;
            bool added = false;
            if (!Semaphore.Wait((int)ComitterTimeoutMS))
            {
                Logger.Warning("Committer queued for more than a " + ComitterTimeoutMS + "ms. Commit canceled.");
                return;
            }
            try
            {
                if (!TcpClient.Connected) Login();
                if (Logger.VerboseMode)
                {
                    List<string> sAddresses = new();
                    foreach (IPAddress address in e.Addresses) sAddresses.Add(address.ToString());
                    Logger.Verbose("Match found, committing " + string.Join(", ", sAddresses) + " under " + e.AddressListName + " to " + Committer.RouterOSIPEndPoint.ToString() + "...");
                }
                foreach (IPAddress address in e.Addresses)
                {
                    string v6 = "";
                    string cidr = "";
                    string comment = "TH: " + string.Join(", ", e.MatchedNames);
                    if (address.AddressFamily == AddressFamily.InterNetworkV6)
                    {
                        v6 = "v6";
                        cidr = "/128";
                    }
                    /*string[] response = TcpClient.SendSentence([
                        "/ip" + v6 + "/firewall/address-list/print",
                        "=.proplist=.id",
                        "?address=" + address.ToString() + cidr,
                        "?list=" + e.AddressListName
                    ]);*/
                    Committed++;
                    /*if (response.Length == 3)
                    {
                        TcpClient.SendSentence([
                            "/ip" + v6 + "/firewall/address-list/set",
                            response[1],
                            "=comment=" + comment,
                            "=timeout=" + ListTTL
                        ]);
                        continue;
                    }*/
                    TcpClient.SendSentence([
                        "/ip" + v6 + "/firewall/address-list/add",
                        "=list=" + e.AddressListName,
                        "=comment=" + comment,
                        "=address=" + address.ToString() + cidr,
                        "=timeout=" + ListTTL
                    ]);
                    added = true;
                }
            }
            catch
            {
                TcpClient.Close();
                Logger.Warning("Connection lost to " + RouterOSIPEndPoint.ToString() + ".");
            }
            finally
            {
                Semaphore.Release();
                if (added)
                {
                    if (Logger.VerboseMode) Logger.Verbose("New entry in IP list, sleeping for " + ComitterDelayMS + "ms for RouterOS to catch up.");
                    //Thread.Sleep((int)ComitterDelayMS);
                }
            }
        }
        public void Dispose()
        {
            if (TcpClient.Connected) TcpClient.Close();
            TcpClient.Dispose();
            Semaphore.Dispose();
        }
    }
}