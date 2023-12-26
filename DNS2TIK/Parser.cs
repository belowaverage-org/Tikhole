using System.Text;

namespace DNS2TIK
{
    public class Parser
    {
        public Parser()
        {
            Program.Forwarder.RecievedResponseData += Forwarder_RecievedResponseData;
        }

        private void Forwarder_RecievedResponseData(object? sender, RecievedResponseDataEventArgs e)
        {
            DNSResourceRecord response = new();
            byte[] bytes = e.Bytes.AsSpan(12).ToArray();
            byte[] part = new byte[0];
            for (int i = 0; i < bytes.Length; i++)
            {
                if (part.Length == 0)
                {
                    part = new byte[bytes[i]];
                }
                else
                {
                    
                }
            }
        }
    }
    public struct DNSResourceRecord
    {
        public string Name;
        public DNSClass Class;
    }
    public enum DNSClass
    {
        A = 1,
        AAAA = 28
    }
}