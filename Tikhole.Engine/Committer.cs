using System.Net;
using System.Net.Sockets;

namespace Tikhole.Engine
{
    public class Committer
    {
        public static string ListTTL = "24h";
        public static string UserName = "Tikhole";
        public static string Password = "";
        public static IPEndPoint RouterOSIPEndPoint = new(IPAddress.Parse("192.168.200.1"), 8728);
        public static TcpClient TcpClient = new();
        private static SemaphoreSlim Semaphore = new(1, 1);
        public Committer()
        {
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
                Logger.Success("Connected to " + RouterOSIPEndPoint.ToString() + ".");
            }
            catch
            {
                Logger.Warning("Failed to connect to " + RouterOSIPEndPoint.ToString() + ".");
            }
        }

        private void Matcher_ResponseMatched(object? sender, ResponseMatchedEventArgs e)
        {
            try
            {
                Semaphore.Wait();
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
                    string comment = "Tikhole: " + string.Join(", ", e.MatchedNames);
                    if (address.AddressFamily == AddressFamily.InterNetworkV6)
                    {
                        v6 = "v6";
                        cidr = "/128";
                    }
                    string[] response = TcpClient.SendSentence([
                        "/ip" + v6 + "/firewall/address-list/print",
                        "=.proplist=.id",
                        "?list=" + e.AddressListName,
                        "?address=" + address.ToString() + cidr
                    ]);
                    if (response.Length == 3)
                    {
                        TcpClient.SendSentence([
                            "/ip" + v6 + "/firewall/address-list/set",
                            response[1],
                            "=comment=" + comment,
                            "=timeout=" + ListTTL
                        ]);
                        continue;
                    }
                    TcpClient.SendSentence([
                        "/ip" + v6 + "/firewall/address-list/add",
                        "=list=" + e.AddressListName,
                        "=comment=" + comment,
                        "=address=" + address.ToString() + cidr,
                        "=timeout=" + ListTTL
                    ]);
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
            }
        }
    }
}