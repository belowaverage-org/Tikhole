namespace DNS2TIK
{
    public class Parser
    {
        public event EventHandler<ParsedResponseDataEventArgs>? ParsedResponseData;
        public Parser()
        {
            Program.Forwarder.RecievedResponseData += Forwarder_RecievedResponseData;
        }

        private void Forwarder_RecievedResponseData(object? sender, RecievedResponseDataEventArgs e)
        {
            int index = 12;
            DNSPacket response = new()
            {
                Questions = new DNSQuestionRecord[e.Bytes.ToUShort(4)],
                Answers = new DNSResourceRecord[e.Bytes.ToUShort(6)]
            };

            for (int i = 0; i < response.Questions.Length; i++)
            {
                response.Questions[i].Name = e.Bytes.ToLabelsString(ref index);
                response.Questions[i].Type = (DNSType)e.Bytes.ToUShort(index + 1);
                response.Questions[i].Class = (DNSClass)e.Bytes.ToUShort(index + 3);
                index += 5;
            }

            for (int i = 0; i < response.Answers.Length; i++)
            {
                response.Answers[i].Name = e.Bytes.ToLabelsString(ref index);
                response.Answers[i].Type = (DNSType)e.Bytes.ToUShort(index + 1);
                response.Answers[i].Class = (DNSClass)e.Bytes.ToUShort(index + 3);
                response.Answers[i].TimeToLive = (int)e.Bytes.ToUInt(index + 5);
                response.Answers[i].Data = new byte[e.Bytes.ToUShort(index + 9)];
                index += 11;
                for (int di = 0; di < response.Answers[i].Data.Length; di++)
                {
                    response.Answers[i].Data[di] = e.Bytes[index++];
                }
            }

            _ = Task.Run(() => ParsedResponseData?.Invoke(null, new() { DNSPacket = response }));
        }
    }
    public class ParsedResponseDataEventArgs : EventArgs
    {
        public required DNSPacket DNSPacket;
    }
    public struct DNSPacket
    {
        public required DNSQuestionRecord[] Questions;
        public required DNSResourceRecord[] Answers;
    }
    public struct DNSQuestionRecord
    {
        public required string Name;
        public required DNSType Type;
        public required DNSClass Class;
    }
    public struct DNSResourceRecord
    {
        public required string Name;
        public required DNSType Type;
        public required DNSClass Class;
        public required int TimeToLive;
        public required byte[] Data;
    }
    public enum DNSType
    {
        A = 1,
        NS = 2,
        CNAME = 5,
        SOA = 6,
        PTR = 12,
        MX = 15,
        TXT = 16,
        AAAA = 28
    }
    public enum DNSClass
    {
        IN = 1
    }
}