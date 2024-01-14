using System.Collections;
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
            Logger.Info("Forwarder set for " + DNSServer.ToString() + ".");
            if (Tikhole.Listener != null) Tikhole.Listener.RecievedRequestData += Listener_RecievedRequestData;
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
        public void Dispose()
        {
            Director.Dispose();
        }
    }
    public class RecievedResponseDataEventArgs : EventArgs
    {
        public required RecievedRequestDataEventArgs RecievedRequestData;
        public required byte[] Data;
    }
    public class Director : IDisposable
    {
        private UdpClient Client = new UdpClient();
        private Dictionary<UInt16, Request> Requests = new();
        private SemaphoreSlim Semaphore = new(1);
        public Director()
        {
            Task.Run(() => {
                while (Client.Client.Poll(-1, SelectMode.SelectRead))
                {
                    IPEndPoint? endpoint = null;
                    byte[] received = Client.Receive(ref endpoint);
                    UInt16 ID = GetID(received);
                    Semaphore.Wait();
                    if (Requests.ContainsKey(ID))
                    {
                        Requests[ID].Response = received;
                        Requests[ID].WaitHandle?.Set();
                    }
                    Semaphore.Release();
                }
            });
        }
        public byte[]? Forward(byte[] Request)
        {
            UInt16 ID = GetID(Request);
            EventWaitHandle waitHandle = new(false, EventResetMode.ManualReset);
            Semaphore.Wait();
            if (Requests.ContainsKey(ID))
            {
                Semaphore.Release();
                return null;
            }
            Requests.Add(ID, new() { WaitHandle = waitHandle });
            Semaphore.Release();
            Client.Send(Request, Forwarder.DNSServer);
            waitHandle.WaitOne(1000);
            waitHandle.Dispose();
            Semaphore.Wait();
            byte[]? response = Requests[ID].Response;
            Requests.Remove(ID);
            Semaphore.Release();
            return response;
        }
        private UInt16 GetID(byte[] Request)
        {
            return BitConverter.ToUInt16(Request, 0);
        }
        public void Dispose()
        {
            Semaphore.Dispose();
            Client.Dispose();
        }
        private record Request
        {
            public EventWaitHandle? WaitHandle;
            public byte[]? Response;
        }
    }
}