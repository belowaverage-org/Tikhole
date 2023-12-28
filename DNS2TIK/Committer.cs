using System.Net;
using System.Net.Sockets;

namespace DNS2TIK
{
    public class Committer
    {
        public static IPEndPoint RouterOSIPEndPoint = new(IPAddress.Parse(""), 8728);
        public static TcpClient TcpClient = new();
        private static SemaphoreSlim Semaphore = new(1, 1);
        public Committer()
        {
            Program.Matcher.ResponseMatched += Matcher_ResponseMatched;
        }

        private void Login()
        {
            Logger.Info("Connecting to " + RouterOSIPEndPoint.ToString() + "...");
            TcpClient = new();
            TcpClient.Connect(RouterOSIPEndPoint);
            TcpClient.SendSentence([
                "/login",
                "=name=",
                "=password="
            ]);
            Logger.Success("Connected to " + RouterOSIPEndPoint.ToString() + ".");
        }

        private void Matcher_ResponseMatched(object? sender, ResponseMatchedEventArgs e)
        {
            try
            {
                Semaphore.Wait();
                if (!TcpClient.Connected) Login();
                foreach (IPAddress address in e.Addresses)
                {
                    Logger.Verbose("Adding " + address.ToString() + " to " + e.AddressListName + "...");
                    if (address.AddressFamily != AddressFamily.InterNetwork) continue;
                    string[] response = TcpClient.SendSentence([
                        "/ip/firewall/address-list/print",
                        "=.proplist=.id",
                        "?list=" + e.AddressListName,
                        "?address=" + address.ToString()
                    ]);
                    if (response.Length == 3)
                    {
                        TcpClient.SendSentence([
                            "/ip/firewall/address-list/set",
                            response[1],
                            "=timeout=24h"
                        ]);
                        continue;
                    }
                    TcpClient.SendSentence([
                        "/ip/firewall/address-list/add",
                        "=list=" + e.AddressListName,
                        "=comment=" + e.MatchedNames[0],
                        "=address=" + address.ToString(),
                        "=timeout=24h"
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