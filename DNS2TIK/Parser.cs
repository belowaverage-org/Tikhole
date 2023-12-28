namespace Tikhole
{
    public class Parser
    {
        public event EventHandler<ParsedResponseDataEventArgs>? ParsedResponseData;
        public Parser()
        {
            Tikhole.Forwarder.RecievedResponseData += Forwarder_RecievedResponseData;
        }
        private void Forwarder_RecievedResponseData(object? sender, RecievedResponseDataEventArgs e)
        {
            _ = Task.Run(() => {
                if (Logger.VerboseMode) Logger.Verbose("Parsing response...");
                int index = 12;
                DNSPacket packet = new()
                {
                    Questions = new DNSQuestionRecord[e.Data.ToUShort(4)],
                    Answers = new DNSResourceRecord[e.Data.ToUShort(6)]
                };
                for (int i = 0; i < packet.Questions.Length; i++)
                {
                    packet.Questions[i].Name = e.Data.ToLabelsString(ref index);
                    packet.Questions[i].Type = (DNSType)e.Data.ToUShort(index + 1);
                    packet.Questions[i].Class = (DNSClass)e.Data.ToUShort(index + 3);
                    index += 5;
                }
                for (int i = 0; i < packet.Answers.Length; i++)
                {
                    packet.Answers[i].Name = e.Data.ToLabelsString(ref index);
                    packet.Answers[i].Type = (DNSType)e.Data.ToUShort(index + 1);
                    packet.Answers[i].Class = (DNSClass)e.Data.ToUShort(index + 3);
                    packet.Answers[i].TimeToLive = (int)e.Data.ToUInt(index + 5);
                    packet.Answers[i].Data = new byte[e.Data.ToUShort(index + 9)];
                    index += 11;
                    packet.Answers[i].DataIndex = index;
                    for (int di = 0; di < packet.Answers[i].Data.Length; di++)
                    {
                        packet.Answers[i].Data[di] = e.Data[index++];
                    }
                }
                ParsedResponseData?.Invoke(null, new() { RecievedResponseData = e, DNSPacket = packet });
            });
        }
    }
    public class ParsedResponseDataEventArgs : EventArgs
    {
        public required RecievedResponseDataEventArgs RecievedResponseData;
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
        public required int DataIndex;
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