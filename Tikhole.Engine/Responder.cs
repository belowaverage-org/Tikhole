using System.Net;

namespace Tikhole.Engine
{
    public class Responder : IDisposable
    {
        public static bool WaitForMatcherAndCommitter = true;
        public Responder()
        {
            Logger.Info("Starting Responder...");
            if (Tikhole.Forwarder != null) Tikhole.Forwarder.RecievedResponseData += Forwarder_RecievedResponseData;
            if (Tikhole.Matcher != null) Tikhole.Matcher.MatchesMatchedAndOrCommitted += Matcher_MatchesMatchedAndOrCommitted;
        }
        public void Dispose()
        {
            Logger.Info("Responder stopped.");
            WaitForMatcherAndCommitter = true;
        }
        public void Respond(Span<byte> Data, IPEndPoint IPEndPoint)
        {
            if (Tikhole.Listener == null) return;
            if (Logger.VerboseMode) Logger.Verbose("Forwarding response to " + IPEndPoint.ToString() + "...");
            Tikhole.Listener.Client.Send(Data, IPEndPoint);
        }
        private void Forwarder_RecievedResponseData(object? sender, RecievedResponseDataEventArgs e)
        {
            if (!WaitForMatcherAndCommitter) _ = Task.Run(() =>
            {
                Respond(e.Data.Span, e.RecievedRequestData.IPEndPoint);
            });
        }
        private void Matcher_MatchesMatchedAndOrCommitted(object? sender, ParsedResponseDataEventArgs e)
        {
            if (WaitForMatcherAndCommitter)
            {
                Respond(e.RecievedResponseData.Data.Span, e.RecievedResponseData.RecievedRequestData.IPEndPoint);
            }
        }
    }
}