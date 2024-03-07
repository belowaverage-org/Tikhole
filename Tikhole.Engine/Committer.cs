using System.Collections;
using System.Net;
using System.Net.Sockets;

namespace Tikhole.Engine
{
    public class Committer : IDisposable
    {
        public static uint ListTTL = 86400;
        public static uint ListTTLUpdateDelay = 3600;
        public static string UserName = "Tikhole";
        public static string Password = "";
        public static uint Committed = 0;
        public static uint Updated = 0;
        public static uint Missed = 0;
        public static uint Delayed = 0;
        public static uint ComitterTimeoutMS = 1000;
        public static uint ComitterDelayMS = 100;
        public static uint TotalInstances { get; set; } = 0;
        public static uint NeededInstances = 2;
        public static IPEndPoint RouterOSIPEndPoint = new(IPAddress.Parse("192.168.200.1"), 8728);
        public static CommitterTrackList TrackList = new();
        public static SemaphoreSlim TrackListSemephore = new(1, 1);
        public TcpClient TcpClient = new();
        private uint InstanceID = 0;
        private uint ResponsesRecieved = 0;
        private SemaphoreSlim TcpClientSemephore = new(1, 1);
        public Committer()
        {
            Logger.Info("Starting Committer...");
            InstanceID = TotalInstances++;
            if (Tikhole.Matcher != null) Tikhole.Matcher.ResponseMatched += Matcher_ResponseMatched;
        }
        public void Dispose()
        {
            if (TcpClient.Connected) TcpClient.Close();
            TcpClient.Dispose();
            TcpClientSemephore.Dispose();
            Committed = 0;
            Updated = 0;
            Missed = 0;
            Delayed = 0;
            TrackList = new();
            TrackListSemephore.Dispose();
            TrackListSemephore = new(1, 1);
            Logger.Info("Committer stopped.");
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
            if (ResponsesRecieved++ % TotalInstances != InstanceID) return;
            if (!TcpClientSemephore.Wait((int)ComitterTimeoutMS))
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
                    string[] reply;
                    string comment = "TH: " + string.Join(", ", e.MatchedNames);
                    CommitterTrackKey ctk = new()
                    {
                        Address = address,
                        List = e.AddressListName
                    };
                    if (TrackListContains(ctk))
                    {
                        CommitterTrackValue ctv = TrackListGet(ctk)!.Value;
                        if (DateTime.Now.CompareTo(ctv.Timeout) >= 0)
                        {
                            TrackListRemove(ctk);
                        }
                        else
                        {
                            if (DateTime.Now.AddSeconds(ListTTL - ListTTLUpdateDelay).CompareTo(ctv.Timeout) >= 0)
                            {
                                reply = ListSet(ctv.ID, address, comment);
                                if (reply.Length == 4)
                                {
                                    Missed++;
                                    TrackListRemove(ctk);
                                }
                                else
                                {
                                    Updated++;
                                    TrackListSet(ctk, new()
                                    {
                                        ID = ctv.ID,
                                        Timeout = DateTime.Now.AddSeconds(ListTTL)
                                    });
                                    continue;
                                }
                            }
                            else
                            {
                                Delayed++;
                                continue;
                            }
                        }
                    }
                    reply = ListAdd(address, e.AddressListName, comment);
                    if (reply.Length == 2 && reply[1].StartsWith("=ret=*"))
                    {
                        Committed++;
                        CommitterTrackValue ctv = new()
                        {
                            ID = reply[1].Replace("=ret=*", ""),
                            Timeout = DateTime.Now.AddSeconds(ListTTL)
                        };
                        TrackListAdd(ctk, ctv);
                        added = true;
                        continue;
                    }
                    Missed++;
                    reply = ListPrint(address, e.AddressListName);
                    if (reply.Length == 5 && reply[1].StartsWith("=.id=*"))
                    {
                        CommitterTrackValue ctv = new()
                        {
                            ID = reply[1].Split('*')[1],
                            Timeout = DateTime.Now.AddSeconds(ListTTL)
                        };
                        string[] test = ListSet(ctv.ID, address, comment);
                        TrackListAdd(ctk, ctv);
                    }
                }
            }
            catch
            {
                TcpClient.Close();
                Logger.Warning("Connection lost to " + RouterOSIPEndPoint.ToString() + ".");
            }
            finally
            {
                TcpClientSemephore.Release();
            }
        }
        private void TrackListSet(CommitterTrackKey Key, CommitterTrackValue Value)
        {
            TrackListSemephore.Wait();
            TrackList[Key] = Value;
            TrackListSemephore.Release();
        }
        private void TrackListRemove(CommitterTrackKey Key)
        {
            TrackListSemephore.Wait();
            TrackList.Remove(Key);
            TrackListSemephore.Release();
        }
        private void TrackListAdd(CommitterTrackKey Key, CommitterTrackValue Value)
        {
            TrackListSemephore.Wait();
            TrackList.Add(Key, Value);
            TrackListSemephore.Release();
        }
        private bool TrackListContains(CommitterTrackKey Key)
        {
            bool result = false;
            TrackListSemephore.Wait();
            result = TrackList.Contains(Key);
            TrackListSemephore.Release();
            return result;
        }
        private CommitterTrackValue? TrackListGet(CommitterTrackKey Key)
        {
            CommitterTrackValue? result;
            TrackListSemephore.Wait();
            result = TrackList[Key];
            TrackListSemephore.Release();
            return result;
        }
        private string[] ListAdd(IPAddress IPAddress, string List, string Comment)
        {
            (string v6, string cidr) = AddressBits(IPAddress);
            string[] result = TcpClient.SendSentence([
                "/ip" + v6 + "/firewall/address-list/add",
                "=list=" + List,
                "=comment=" + Comment,
                "=address=" + IPAddress.ToString() + cidr,
                "=timeout=" + ListTTL.ToString()
            ]);
            if (Logger.VerboseMode) Logger.Verbose("New entry in IP list, sleeping for " + ComitterDelayMS + "ms for RouterOS to catch up.");
            Thread.Sleep((int)ComitterDelayMS);
            return result;

        }
        private string[] ListSet(string ID, IPAddress IPAddress, string Comment)
        {
            (string v6, string cidr) = AddressBits(IPAddress);
            return TcpClient.SendSentence([
                "/ip" + v6 + "/firewall/address-list/set",
                "=numbers=*" + ID,
                "=comment=" + Comment,
                "=address=" + IPAddress.ToString() + cidr,
                "=timeout=" + ListTTL.ToString()
            ]);
        }
        private string[] ListPrint(IPAddress IPAddress, string List)
        {
            (string v6, string cidr) = AddressBits(IPAddress);
            return TcpClient.SendSentence([
                "/ip" + v6 + "/firewall/address-list/print",
                "=.proplist=.id,timeout",
                "?list=" + List,
                "?address=" + IPAddress.ToString() + cidr
            ]);
        }
        private (string, string) AddressBits(IPAddress IPAddress)
        {
            string v6 = "";
            string cidr = "";
            if (IPAddress.AddressFamily == AddressFamily.InterNetworkV6)
            {
                v6 = "v6";
                cidr = "/128";
            }
            return (v6, cidr);
        }
    }
    public class CommitterTrackList : Hashtable
    {
        public CommitterTrackValue? this[CommitterTrackKey Key] { get => (CommitterTrackValue?)base[Key]; set => base[Key] = value; }
        public CommitterTrackList() : base() { }
        public void Add(CommitterTrackKey Key, CommitterTrackValue Value)
        {
            if (!Contains(Key)) base.Add(Key, Value);
        }
        public void Remove(CommitterTrackKey Key)
        {
            base.Remove(Key);
        }
        public bool Contains(CommitterTrackKey Key)
        {
            return base.Contains(Key);
        }
    }
    
    public struct CommitterTrackKey
    {
        public string List;
        public IPAddress Address;
        public override int GetHashCode()
        {
            return HashCode.Combine(List.GetHashCode(), Address.GetHashCode());
        }
    }
    public struct CommitterTrackValue
    {
        public string ID;
        public DateTime Timeout;
    }
}