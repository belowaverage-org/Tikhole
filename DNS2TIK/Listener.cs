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
            Client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            Client.Client.Bind(IPEndPoint);
            _ = Task.Run(() =>
            {
                while (Client != null)
                {
                    IPEndPoint? ipEndPoint = null;
                    byte[] bytes = Client.Receive(ref ipEndPoint);
                    _ = Task.Run(() => RecievedRequestData?.Invoke(null, new(ipEndPoint, bytes)));
                }
            });
        }
    }
    public class RecievedRequestDataEventArgs : EventArgs
    {
        public IPEndPoint IPEndPoint;
        public byte[] Bytes;
        public RecievedRequestDataEventArgs(IPEndPoint IPEndPoint, byte[] Bytes)
        {
            this.IPEndPoint = IPEndPoint;
            this.Bytes = Bytes;
        }
    }
}