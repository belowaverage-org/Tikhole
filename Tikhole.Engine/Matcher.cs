using System.Net;
using System.Text.RegularExpressions;

namespace Tikhole.Engine
{
    public class Matcher
    {
        public static uint Matches = 0;
        public static MatchTable MatchTable = new()
        {
            new ("Apple", new("^.*\\.?apple\\.com$")),
            new ("Google", new("^.*\\.?google\\.com$")),
            new ("Reddit", new ("^.*\\.?reddit\\.com$")),
            new ("Youtube", new ("^.*\\.?youtube\\.com$")),
            new ("Microsoft", new("^.*\\.?microsoft\\.com$")),
            new ("Facebook", new("^.*\\.?facebook\\.com$"))
        };
        public event EventHandler<ResponseMatchedEventArgs>? ResponseMatched;
        public event EventHandler<ParsedResponseDataEventArgs>? MatchesMatchedAndOrCommitted;
        public Matcher()
        {
            if (Tikhole.Parser != null) Tikhole.Parser.ParsedResponseData += Parser_ParsedResponseData;
        }
        private void Parser_ParsedResponseData(object? sender, ParsedResponseDataEventArgs e)
        {
            try
            {
                if (e.DNSPacket.Answers.Length == 0)
                {
                    if (Logger.VerboseMode) Logger.Verbose("Response has no answers.");
                    return;
                }
                if (Logger.VerboseMode)
                {
                    List<string> names = new();
                    foreach (DNSResourceRecord answer in e.DNSPacket.Answers) names.Add(answer.Name);
                    Logger.Verbose("Response parsed as " + string.Join(", ", names) + ", checking for matches...");
                }
                foreach (KeyValuePair<string, Regex> matcher in MatchTable)
                {
                    if (matcher.Key == null || matcher.Key == string.Empty || matcher.Value == null) continue;
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
                    Matches++;
                }
            }
            finally
            {
                MatchesMatchedAndOrCommitted?.Invoke(null, e);
            }
        }
    }
    public class MatchTable : List<KeyValuePair<string, Regex>> { }
    public class ResponseMatchedEventArgs : EventArgs
    {
        public required ParsedResponseDataEventArgs ParsedResponseData;
        public required string AddressListName;
        public required string[] MatchedNames;
        public required string[] Aliases;
        public required IPAddress[] Addresses;
    }
}