using System.Net;

namespace Tikhole.Engine
{
    public class Responder
    {
        public static bool WaitForMatcherAndCommitter = true;
        public Responder()
        {
            if (Tikhole.Forwarder != null) Tikhole.Forwarder.RecievedResponseData += Forwarder_RecievedResponseData;
            if (Tikhole.Matcher != null) Tikhole.Matcher.MatchesMatchedAndOrCommitted += Matcher_MatchesMatchedAndOrCommitted;
        }
        public void Respond(byte[] Data, IPEndPoint IPEndPoint)
        {
            if (Tikhole.Listener == null) return;
            if (Logger.VerboseMode) Logger.Verbose("Recieved response from " + Forwarder.DNSServer.ToString() + ", forwarding to " + IPEndPoint.ToString() + "...");
            Listener.Client.Send(Data, IPEndPoint);
        }
        private void Forwarder_RecievedResponseData(object? sender, RecievedResponseDataEventArgs e)
        {
            if (!WaitForMatcherAndCommitter) _ = Task.Run(() =>
            {
                Respond(e.Data, e.RecievedRequestData.IPEndPoint);
            });
        }
        private void Matcher_MatchesMatchedAndOrCommitted(object? sender, ParsedResponseDataEventArgs e)
        {
            if (WaitForMatcherAndCommitter)
            {
                Respond(e.RecievedResponseData.Data, e.RecievedResponseData.RecievedRequestData.IPEndPoint);
            }
        }
    }
}