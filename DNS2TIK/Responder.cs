namespace DNS2TIK
{
    public class Responder
    {
        public Responder()
        {
            Program.Forwarder.RecievedResponseData += Forwarder_RecievedResponseData;
        }

        private void Forwarder_RecievedResponseData(object? sender, RecievedResponseDataEventArgs e)
        {
            Program.Listener.Client.Send(e.Bytes, e.RecievedRequestData.IPEndPoint);
        }
    }
}