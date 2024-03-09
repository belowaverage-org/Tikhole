using System.Net;
using System.Net.Sockets;

namespace Tikhole.Engine
{
    public class Forwarder : IDisposable
    {
        public static IPEndPoint DNSServer = new(IPAddress.Parse("1.1.1.1"), 53);
        private static Director Director = new();
        public event EventHandler<RecievedResponseDataEventArgs>? RecievedResponseData;
        public Forwarder()
        {
            Logger.Info("Starting Forwarder: Destination set for " + DNSServer.ToString() + "...");
            if (Tikhole.Listener != null) Tikhole.Listener.RecievedRequestData += Listener_RecievedRequestData;
        }
        public void Dispose()
        {
            Director.Dispose();
            Director = new();
            Logger.Info("Forwarder stopped.");
        }
        private void Listener_RecievedRequestData(object? sender, RecievedRequestDataEventArgs e)
        {
            if (Logger.VerboseMode) Logger.Verbose("Recieved request from " + e.IPEndPoint.ToString() + ", forwarding to " + DNSServer.ToString() + "...");
            byte[]? response = Director.Forward(e.Data);
            if (response == null)
            {
                Logger.Info("Request from " + e.IPEndPoint.ToString() + " timed out or experienced an error when sent to " + DNSServer.ToString() + ".");
                return;
            }
            RecievedResponseData?.Invoke(null, new() { RecievedRequestData = e, Data = response });
        }
    }
    public class RecievedResponseDataEventArgs : EventArgs
    {
        public required RecievedRequestDataEventArgs RecievedRequestData;
        public required Memory<byte> Data;
    }
    public class Director : IDisposable
    {
        private UdpClient Client = new UdpClient();
        private Dictionary<ushort, Request> Requests = new();
        private SemaphoreSlim RequestSemaphore = new(1);
        private SemaphoreSlim IDCounterSemaphore = new(1);
        private ushort IDCounter = 0;
        public Director()
        {
            new Task(() => {
                while (Client.Client.Poll(-1, SelectMode.SelectRead))
                {
                    IPEndPoint? endpoint = null;
                    byte[] received = Client.Receive(ref endpoint);
                    ushort ID = GetID(received);
                    RequestSemaphore.Wait();
                    if (Requests.ContainsKey(ID))
                    {
                        Requests[ID].Response = received;
                        Requests[ID].WaitHandle?.Set();
                    }
                    RequestSemaphore.Release();
                }
            }, TaskCreationOptions.LongRunning).Start();
        }
        public void Dispose()
        {
            RequestSemaphore.Dispose();
            IDCounterSemaphore.Dispose();
            Client.Dispose();
        }
        public byte[]? Forward(Memory<byte> Request)
        {
            IDCounterSemaphore.Wait();
            ushort ID = IDCounter++;
            IDCounterSemaphore.Release();
            RequestSemaphore.Wait();
            if (Requests.ContainsKey(ID))
            {
                RequestSemaphore.Release();
                return null;
            }
            EventWaitHandle waitHandle = new(false, EventResetMode.ManualReset);
            Requests.Add(ID, new() { WaitHandle = waitHandle, OriginalID = GetID(Request) });
            RequestSemaphore.Release();
            SetID(Request, ID);
            Client.Send(Request.Span, Forwarder.DNSServer);
            waitHandle.WaitOne(1000);
            waitHandle.Dispose();
            RequestSemaphore.Wait();
            byte[]? response = Requests[ID].Response;
            SetID(response, Requests[ID].OriginalID);
            Requests.Remove(ID);
            RequestSemaphore.Release();
            return response;
        }
        private ushort GetID(Memory<byte> DNSPacket)
        {
            return BitConverter.ToUInt16([DNSPacket.Span[1], DNSPacket.Span[0]]);
        }
        private void SetID(Memory<byte> DNSPacket, ushort ID)
        {
            byte[] bytes = BitConverter.GetBytes(ID);
            DNSPacket.Span[0] = bytes[1];
            DNSPacket.Span[1] = bytes[0];
        }
        private record Request
        {
            public EventWaitHandle? WaitHandle;
            public ushort OriginalID;
            public byte[]? Response;
        }
    }
}