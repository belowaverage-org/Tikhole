using System.Net;
using System.Net.Sockets;

namespace Tikhole
{
    public class Forwarder
    {
        public IPEndPoint DNSServer = new(IPAddress.Parse("9.9.9.9"), 53);
        public event EventHandler<RecievedResponseDataEventArgs>? RecievedResponseData;
        public Forwarder()
        {
            Tikhole.Listener.RecievedRequestData += Listener_RecievedRequestData;
        }
        private void Listener_RecievedRequestData(object? sender, RecievedRequestDataEventArgs e)
        {
            if (Logger.VerboseMode) Logger.Verbose("Recieved request from " + e.IPEndPoint.ToString() + ", forwarding to " + Tikhole.Forwarder.DNSServer.ToString() + "...");
            IPEndPoint? responseEndPoint = null;
            UdpClient udpClient = new();
            udpClient.Connect(DNSServer);
            udpClient.Send(e.Data);
            byte[] responseBytes = udpClient.Receive(ref responseEndPoint);
            udpClient.Close();
            RecievedResponseData?.Invoke(null, new() { RecievedRequestData = e, Data = responseBytes });
        }
    }
    public class RecievedResponseDataEventArgs : EventArgs
    {
        public required RecievedRequestDataEventArgs RecievedRequestData;
        public required byte[] Data;
    }
}