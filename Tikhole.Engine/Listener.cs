using System.Net;
using System.Net.Sockets;

namespace Tikhole.Engine
{
    public class Listener
    {
        public static UdpClient Client = new();
        public static IPEndPoint IPEndPoint = new(IPAddress.Any, 53);
        public static uint Requests = 0;
        public event EventHandler<RecievedRequestDataEventArgs>? RecievedRequestData;
        public Listener()
        {
            try
            {
                Logger.Info("Starting listnener on " + IPEndPoint.ToString() + "...");
                Client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                Client.Client.Bind(IPEndPoint);
                Task listener = new Task(() =>
                {
                    while (Client != null)
                    {
                        IPEndPoint? ipEndPoint = null;
                        try
                        {
                            byte[] bytes = Client.Receive(ref ipEndPoint);
                            _ = Task.Run(() => RecievedRequestData?.Invoke(null, new() { IPEndPoint = ipEndPoint, Data = bytes }));
                            Requests++;
                        }
                        catch
                        {
                            Logger.Warning("Error receiving request.");
                        }
                    }
                }, TaskCreationOptions.LongRunning);
                listener.Start();
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