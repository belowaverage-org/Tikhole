using System.Net;
using System.Net.Sockets;

namespace Tikhole.Engine
{
    public class Syncer : IDisposable
    {
        private TcpClient? TcpClient;
        private System.Timers.Timer Timer = new();
        private bool Running = false;
        public static uint SyncerIntervalSeconds = 300;
        public Syncer()
        {
            Timer.Elapsed += SyncNow;
            Timer.Enabled = true;
            Timer.Interval = SyncerIntervalSeconds * 1000;
            Timer.Start();
            SyncNow();
        }
        public async void SyncNow(object? o = null, object? e = null)
        {
            if (Running) return;
            Running = true;
            await Task.Run(() => {
                Logger.Info("Connecting to " + Committer.RouterOSIPEndPoint.ToString() + "...");
                TcpClient = new();
                TcpClient.Connect(Committer.RouterOSIPEndPoint);
                TcpClient.SendSentence([
                    "/login",
                    "=name=" + Committer.UserName,
                    "=password=" + Committer.Password
                ]);
                Logger.Info("Downloading ipv4 address lists...");
                string[] list4 = TcpClient.SendSentence([
                    "/ip/firewall/address-list/print",
                    "=.proplist=.id,address,list,timeout"
                ]);
                Logger.Info("Downloading ipv6 address lists...");
                string[] list6 = TcpClient.SendSentence([
                    "/ipv6/firewall/address-list/print",
                    "=.proplist=.id,address,list,timeout"
                ]);
                Logger.Info("Disconnecting from " + Committer.RouterOSIPEndPoint.ToString() + "...");
                TcpClient.Close();
                TcpClient.Dispose();
                Logger.Info("Reading lists...");
                string[] list = list4.Concat(list6).ToArray();
                bool skip = false;
                CommitterTrackList stagedTrackList = new();
                CommitterTrackKey CTK = new();
                CommitterTrackValue CTV = new();
                foreach (string word in list)
                {
                    if (word == "!re") continue;
                    if (word.StartsWith("=.id=*"))
                    {
                        CTV.ID = word.Replace("=.id=*", "");
                    }
                    if (word.StartsWith("=address="))
                    {
                        if (IPAddress.TryParse(word.Replace("=address=", "").Replace("/128", ""), out IPAddress? ipAddress))
                        {
                            CTK.Address = ipAddress;
                        }
                        else
                        {
                            skip = true;
                        }
                    }
                    if (word.StartsWith("=list="))
                    {
                        CTK.List = word.Replace("=list=", "");
                    }
                    if (word.StartsWith("=timeout="))
                    {
                        string sNumber = string.Empty;
                        string time = word.Replace("=timeout=", "");
                        DateTime timeout = DateTime.Now;
                        foreach (char character in time)
                        {
                            if ("1234567890".Contains(character)) sNumber += character;
                            if ("wdhms".Contains(character) && uint.TryParse(sNumber, out uint number))
                            {
                                if (character == 'w') timeout = timeout.AddDays(number * 7);
                                if (character == 'd') timeout = timeout.AddDays(number);
                                if (character == 'h') timeout = timeout.AddHours(number);
                                if (character == 'm') timeout = timeout.AddMinutes(number);
                                if (character == 's') timeout = timeout.AddSeconds(number);
                                sNumber = string.Empty;
                            }
                        }
                        CTV.Timeout = timeout;
                    }
                    if (word == "")
                    {
                        if (skip)
                        {
                            skip = false;
                            continue;
                        }
                        if (CTV.Timeout == new DateTime()) continue;
                        stagedTrackList.Add(CTK, CTV);
                    }
                }
                Logger.Info("Swapping lists...");
                Committer.TrackListSemephore.Wait();
                Committer.TrackList = stagedTrackList;
                Committer.TrackListSemephore.Release();
                Logger.Success("Done updating committer tracking list.");
            });
            Running = false;
        }
        public void Dispose()
        {
            TcpClient?.Dispose();
            Timer.Stop();
            Timer.Dispose();
        }
    }
}