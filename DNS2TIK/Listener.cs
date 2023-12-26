using System.Net;
using System.Net.Sockets;

namespace DNS2TIK
{
    public class Listener
    {
        public UdpClient Client = new();
        public event EventHandler<RecievedDataEventArgs> RecievedData;
        public IPEndPoint IPEndPoint = new(IPAddress.Any, 53);
        public Listener()
        {
            Client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            Client.Client.Bind(IPEndPoint);
            _ = Task.Run(() =>
            {
                while (Client != null)
                {
                    RecievedDataEventArgs e = new();
                    e.Bytes = Client.Receive(ref IPEndPoint);
                    RecievedData?.Invoke(null, e);
                }
            });
        }
    }
    public class RecievedDataEventArgs : EventArgs
    {
        public byte[] Bytes { get; set; }
    }
}