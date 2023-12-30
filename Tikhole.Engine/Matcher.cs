using System.Net;
using System.Text.RegularExpressions;

namespace Tikhole.Engine
{
    public class Matcher
    {
        public static MatchTable MatchTable = new()
        {
            { "Apple", new("^.*\\.?apple\\.com$") },
            { "Google", new("^.*\\.?google\\.com$") },
            { "Reddit", new("^.*\\.?reddit\\.com$") },
            { "Youtube", new("^.*\\.?youtube\\.com$") },
            { "Microsoft", new("^.*\\.?microsoft\\.com$") },
            { "Facebook", new("^.*\\.?facebook\\.com$") }
        };
        public event EventHandler<ResponseMatchedEventArgs>? ResponseMatched;
        public Matcher()
        {
            if (Tikhole.Parser != null) Tikhole.Parser.ParsedResponseData += Parser_ParsedResponseData;
        }
        private void Parser_ParsedResponseData(object? sender, ParsedResponseDataEventArgs e)
        {
            if (Logger.VerboseMode)
            {
                if (e.DNSPacket.Answers.Length == 0)
                {
                    Logger.Verbose("Response has no answers.");
                    return;
                }
                List<string> names = new();
                foreach (DNSResourceRecord answer in e.DNSPacket.Answers) names.Add(answer.Name);
                Logger.Verbose("Response parsed as " + string.Join(", ", names) + ", checking for matches...");
            }
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
                ResponseMatched?.Invoke(null, new()
                {
                    ParsedResponseData = e,
                    AddressListName = matcher.Key,
                    MatchedNames = matchedNames.ToArray(),
                    Aliases = aliases.ToArray(),
                    Addresses = addresses.ToArray()
                });
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