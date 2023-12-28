using System.Net;
using System.Text.RegularExpressions;

namespace DNS2TIK
{
    public class Matcher
    {
        public static MatchTable MatchTable = new()
        {
            { "DNS_Apple", new("^.*\\.?apple\\.com$") },
            { "DNS_Google", new("^.*\\.?google\\.com$") },
            { "DNS_Reddit", new("^.*\\.?reddit\\.com$") },
            { "DNS_Youtube", new("^.*\\.?youtube\\.com$") },
            { "DNS_Microsoft", new("^.*\\.?microsoft\\.com$") },
            { "DNS_Facebook", new("^.*\\.?facebook\\.com$") },
            { "DNS_Everything", new(".*") }
        };
        public event EventHandler<ResponseMatchedEventArgs>? ResponseMatched;
        public Matcher()
        {
            Program.Parser.ParsedResponseData += Parser_ParsedResponseData;
        }
        private void Parser_ParsedResponseData(object? sender, ParsedResponseDataEventArgs e)
        {
            foreach (KeyValuePair<string, Regex> matcher in MatchTable)
            {
                List<string> matchedNames = new();
                List<string> aliases = new();
                List<IPAddress> addresses = new();
                foreach (DNSResourceRecord answer in e.DNSPacket.Answers)
                {
                    if (!matchedNames.Contains(answer.Name) && matcher.Value.IsMatch(answer.Name)) matchedNames.Add(answer.Name);
                }
                while (true)
                {
                    bool matched = false;
                    foreach (DNSResourceRecord answer in e.DNSPacket.Answers)
                    {
                        if (answer.Type != DNSType.CNAME) continue;
                        if (matcher.Value.IsMatch(answer.Name) || aliases.Contains(answer.Name))
                        {
                            int index = answer.DataIndex;
                            string data = e.RecievedResponseData.Data.ToLabelsString(ref index);
                            if (!aliases.Contains(data))
                            {
                                aliases.Add(data);
                                matched = true;
                            }
                        }
                    }
                    if (!matched) break;
                }
                foreach (DNSResourceRecord answer in e.DNSPacket.Answers)
                {
                    if (answer.Type != DNSType.A && answer.Type != DNSType.AAAA) continue;
                    if (matcher.Value.IsMatch(answer.Name) || aliases.Contains(answer.Name))
                    {
                        addresses.AddRange(answer.ToAddresses());
                    }
                }
                if (addresses.Count == 0) continue;
                _ = Task.Run(() => ResponseMatched?.Invoke(null, new()
                {
                    ParsedResponseData = e,
                    AddressListName = matcher.Key,
                    MatchedNames = matchedNames.ToArray(),
                    Aliases = aliases.ToArray(),
                    Addresses = addresses.ToArray()
                }));
            }
        }
    }
    public class MatchTable : Dictionary<string, Regex> { }
    public class ResponseMatchedEventArgs : EventArgs
    {
        public required ParsedResponseDataEventArgs ParsedResponseData;
        public required string AddressListName;
        public required string[] MatchedNames;
        public required string[] Aliases;
        public required IPAddress[] Addresses;
    }
}