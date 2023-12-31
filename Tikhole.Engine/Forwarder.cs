using System.Net;
using System.Net.Sockets;

namespace Tikhole.Engine
{
    public class Forwarder
    {
        public static IPEndPoint DNSServer = new(IPAddress.Parse("1.1.1.1"), 53);
        public event EventHandler<RecievedResponseDataEventArgs>? RecievedResponseData;
        public Forwarder()
        {
            Logger.Info("Forwarder set for " + DNSServer.ToString() + ".");
            if (Tikhole.Listener != null) Tikhole.Listener.RecievedRequestData += Listener_RecievedRequestData;
        }
        private void Listener_RecievedRequestData(object? sender, RecievedRequestDataEventArgs e)
        {
            if (Logger.VerboseMode) Logger.Verbose("Recieved request from " + e.IPEndPoint.ToString() + ", forwarding to " + DNSServer.ToString() + "...");
            IPEndPoint? responseEndPoint = null;
            UdpClient udpClient = new();
            udpClient.Connect(DNSServer);
            udpClient.Send(e.Data);
            byte[] responseBytes = udpClient.Receive(ref responseEndPoint);
            udpClient.Close();
            RecievedResponseData?.Invoke(null, new() { RecievedRequestData = e, Data = responseBytes});
        }
    }
    public class RecievedResponseDataEventArgs : EventArgs
    {
        public required RecievedRequestDataEventArgs RecievedRequestData;
        public required byte[] Data;
    }
}