namespace Tikhole.Engine
{
    public class Parser : IDisposable
    {
        public event EventHandler<ParsedResponseDataEventArgs>? ParsedResponseData;
        public Parser()
        {
            Logger.Info("Starting Parser...");
            if (Tikhole.Forwarder != null) Tikhole.Forwarder.RecievedResponseData += Forwarder_RecievedResponseData;
        }
        public void Dispose()
        {
            Logger.Info("Parser stopped.");
        }
        private void Forwarder_RecievedResponseData(object? sender, RecievedResponseDataEventArgs e)
        {
            _ = Task.Run(() => {
                if (Logger.VerboseMode) Logger.Verbose("Parsing response...");
                int index = 12;
                Span<byte> span = e.Data.Span;
                DNSPacket packet = new()
                {
                    Questions = new DNSQuestionRecord[span.ToUShort(4)],
                    Answers = new DNSResourceRecord[span.ToUShort(6)]
                };
                for (int i = 0; i < packet.Questions.Length; i++)
                {
                    packet.Questions[i] = new()
                    {
                        Name = span.ToLabelsString(ref index),
                        Type = (DNSType)span.ToUShort(index + 1),
                        Class = (DNSClass)span.ToUShort(index + 3)
                    };
                    index += 5;
                }
                for (int i = 0; i < packet.Answers.Length; i++)
                {
                    packet.Answers[i] = new()
                    {
                        Name = span.ToLabelsString(ref index),
                        Type = (DNSType)span.ToUShort(index + 1),
                        Class = (DNSClass)span.ToUShort(index + 3),
                        TimeToLive = (int)span.ToUInt(index + 5),
                        Data = e.Data.Slice(index + 11, span.ToUShort(index + 9)),
                        DataIndex = index += 11
                    };
                    index += packet.Answers[i].Data.Length;
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
    public readonly struct DNSPacket
    {
        public readonly DNSQuestionRecord[] Questions { get; init; }
        public readonly DNSResourceRecord[] Answers { get; init; }
    }
    public readonly struct DNSQuestionRecord
    {
        public readonly string Name { get; init; }
        public readonly DNSType Type { get; init; }
        public readonly DNSClass Class { get; init; }
    }
    public readonly struct DNSResourceRecord
    {
        public readonly string Name { get; init; }
        public readonly DNSType Type { get; init; }
        public readonly DNSClass Class { get; init; }
        public readonly int TimeToLive { get; init; }
        public readonly int DataIndex { get; init; }
        public readonly Memory<byte> Data { get; init; }
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