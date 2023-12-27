﻿using System.Net;
using System.Net.Sockets;

namespace DNS2TIK
{
    public class Forwarder
    {
        public IPEndPoint DNSServer = new(IPAddress.Parse("9.9.9.9"), 53);
        public event EventHandler<RecievedResponseDataEventArgs>? RecievedResponseData;
        public Forwarder()
        {
            Program.Listener.RecievedRequestData += Listener_RecievedRequestData;
        }

        private void Listener_RecievedRequestData(object? sender, RecievedRequestDataEventArgs e)
        {
            IPEndPoint? responseEndPoint = null;
            UdpClient udpClient = new();
            udpClient.Connect(DNSServer);
            udpClient.Send(e.Bytes);
            byte[] responseBytes = udpClient.Receive(ref responseEndPoint);
            udpClient.Close();
            _ = Task.Run(() => RecievedResponseData?.Invoke(null, new() { RecievedRequestData = e, Bytes = responseBytes }));
        }
    }
    public class RecievedResponseDataEventArgs : EventArgs
    {
        public required RecievedRequestDataEventArgs RecievedRequestData;
        public required byte[] Bytes;
    }
}