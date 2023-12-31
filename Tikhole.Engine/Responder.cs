namespace Tikhole.Engine
{
    public class Responder
    {
        public Responder()
        {
            if (Tikhole.Forwarder != null) Tikhole.Forwarder.RecievedResponseData += Forwarder_RecievedResponseData;
        }
        private void Forwarder_RecievedResponseData(object? sender, RecievedResponseDataEventArgs e)
        {
            _ = Task.Run(() => { 
                if (Tikhole.Listener == null) return;
                if (Logger.VerboseMode) Logger.Verbose("Recieved response from " + Forwarder.DNSServer.ToString() + ", forwarding to " + e.RecievedRequestData.IPEndPoint.ToString() + "...");
                Listener.Client.Send(e.Data, e.RecievedRequestData.IPEndPoint);
            });
        }
    }
}