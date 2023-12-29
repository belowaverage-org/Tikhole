using System.Net;
using System.Net.Sockets;

namespace Tikhole
{
    public class Listener
    {
        public UdpClient Client = new();
        public event EventHandler<RecievedRequestDataEventArgs>? RecievedRequestData;
        public IPEndPoint IPEndPoint = new(IPAddress.Any, 53);
        public Listener()
        {
            try
            {
                Logger.Info("Starting listnener on " + IPEndPoint.ToString() + "...");
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
                        catch
                        {
                            Logger.Warning("Error receiving request.");
                        }
                    }
                });
                Logger.Success("Listener started on " + IPEndPoint.ToString() + ".");
            }
            catch
            {
                Logger.Error("Could not start listener on " + IPEndPoint.ToString() + ".");
            }
        }
    }
    public class RecievedRequestDataEventArgs : EventArgs
    {
        public required IPEndPoint IPEndPoint;
        public required byte[] Data;
    }
}