using System.Net;
using System.Net.Sockets;

namespace DNS2TIK
{
    public class Listener
    {
        public UdpClient Client = new();
        public event EventHandler<RecievedRequestDataEventArgs>? RecievedRequestData;
        public IPEndPoint IPEndPoint = new(IPAddress.Any, 53);
        public Listener()
        {
            Logger.Info("Starting DNS server on " + IPEndPoint.ToString() + "...");
            Client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            Client.Client.Bind(IPEndPoint);
            _ = Task.Run(() =>
            {
                while (Client != null)
                {
                    IPEndPoint? ipEndPoint = null;
                    try
                    {
                        byte[] bytes = Client.Receive(ref ipEndPoint);
                        _ = Task.Run(() => RecievedRequestData?.Invoke(null, new() { IPEndPoint = ipEndPoint, Data = bytes }));
                    }
                    catch { }
                }
            });
        }
    }
    public class RecievedRequestDataEventArgs : EventArgs
    {
        public required IPEndPoint IPEndPoint;
        public required byte[] Data;
    }
}