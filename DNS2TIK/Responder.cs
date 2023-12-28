namespace Tikhole
{
    public class Responder
    {
        public Responder()
        {
            Tikhole.Forwarder.RecievedResponseData += Forwarder_RecievedResponseData;
        }
        private void Forwarder_RecievedResponseData(object? sender, RecievedResponseDataEventArgs e)
        {
            if (Logger.VerboseMode) Logger.Verbose("Recieved response from " + Tikhole.Forwarder.DNSServer.ToString() + ", forwarding to " + e.RecievedRequestData.IPEndPoint.ToString() + "...");
            Tikhole.Listener.Client.Send(e.Data, e.RecievedRequestData.IPEndPoint);
        }
    }
}