using System.Net;
using System.Net.Sockets;

namespace DNS2TIK
{
    public class Listener
    {
        public UdpClient Client = new();
        public event EventHandler RecievedData;
        public IPEndPoint IPEndPoint = new(IPAddress.Any, 53);
        public Listener()
        {
            Client.Connect(IPEndPoint);
            _ = Task.Run(() =>
            {
                while (Client != null)
                {
                    Client.Receive(ref IPEndPoint);
                    RecievedData?.Invoke(null, EventArgs.Empty);
                }
            });
        }
    }
}