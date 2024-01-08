using System.ComponentModel;
using System.Net;
using System.Text.RegularExpressions;

namespace Tikhole.Engine
{
    public class Matcher
    {
        public static uint Matches = 0;
        public static Rules Rules = new();
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
                foreach (Rule rule in Rules)
                {
                    List<string> matchedNames = new();
                    List<string> aliases = new();
                    List<IPAddress> addresses = new();
                    foreach (DNSResourceRecord answer in e.DNSPacket.Answers)
                    {
                        if (!matchedNames.Contains(answer.Name) && rule.Matches(answer.Name)) matchedNames.Add(answer.Name);
                    }
                    while (true)
                    {
                        bool matched = false;
                        foreach (DNSResourceRecord answer in e.DNSPacket.Answers)
                        {
                            if (answer.Type != DNSType.CNAME) continue;
                            if (rule.Matches(answer.Name) || aliases.Contains(answer.Name))
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
                        if (rule.Matches(answer.Name) || aliases.Contains(answer.Name))
                        {
                            addresses.AddRange(answer.ToAddresses());
                        }
                    }
                    if (addresses.Count == 0) continue;
                    ResponseMatched?.Invoke(null, new()
                    {
                        ParsedResponseData = e,
                        AddressListName = rule.Name,
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
    public class MatchTableRegex : List<KeyValuePair<string, Regex>> { }
    public class Rules : List<Rule> { }
    [DisplayName("Host File")]
    public class RuleHashSetDownloadableHostFile : RuleHashSetDownloadable
    {
        public RuleHashSetDownloadableHostFile(string Name, Uri Uri, System.Timers.Timer UpdateTimer) : base(Name, Uri, UpdateTimer) { }
        private static Regex DomainNameMatcher = new("(?:^[0-9a-fA-F.:]*?\\s+)([a-zA-Z0-9.-]*)(?:$)", RegexOptions.Multiline | RegexOptions.Compiled);
        public override void UpdateList(object? a = null, object? b = null)
        {
            base.UpdateList(a, b);
            try
            {
                Logger.Info("Downloading host file from: " + Uri.ToString() + "...");
                Task<string> request = HttpClient.GetStringAsync(Uri);
                request.Wait();
                Logger.Info("Importing hosts from host file: " + Uri.ToString() + "...");
                MatchCollection matches = DomainNameMatcher.Matches(request.Result);
                foreach (Match match in matches)
                {
                    if (match.Groups[1] != null)
                    {
                        string domain = match.Groups[1].Value;
                        if (!List.Contains(domain)) List.Add(domain);
                    }
                }
                Logger.Success("List imported from host file: " + Uri.ToString() + ".");
            }
            catch
            {
                Logger.Warning("Failed to import host file: " + Uri.ToString() + ".");
            }
        }
    }
    public abstract class RuleHashSetDownloadable : RuleHashSet
    {
        public Uri Uri;
        public System.Timers.Timer UpdateTimer;
        private protected static HttpClient HttpClient = new();
        public RuleHashSetDownloadable(string Name, Uri Uri, System.Timers.Timer UpdateTimer) : base(Name)
        {
            this.Uri = Uri;
            this.UpdateTimer = UpdateTimer;
            UpdateTimer.Elapsed += UpdateList;
            UpdateTimer.Enabled = true;
            UpdateTimer.Start();
            UpdateList();
        }
        public virtual void UpdateList(object? a = null, object? b = null)
        {
            List.Clear();
        }
        public override bool Matches(string Hostname)
        {
            return List.Contains(Hostname);
        }
        public override void Dispose()
        {
            UpdateTimer.Stop();
            UpdateTimer.Dispose();
        }
    }
    [DisplayName("Regular Expression")]
    public class RuleRegex : Rule
    {
        public Regex Regex;
        public RuleRegex(string Name, Regex Regex) : base(Name)
        {
            this.Regex = Regex;
        }
        public override bool Matches(string Hostname)
        {
            return Regex.IsMatch(Hostname);
        }
        public override void Dispose() { }
    }
    public abstract class RuleHashSet : Rule
    {
        protected RuleHashSet(string Name) : base(Name) { }
        public HashSet<string> List = new();
    }
    public abstract class Rule : IDisposable
    {
        public string Name;
        public Rule(string Name)
        {
            this.Name = Name;
        }
        public abstract void Dispose();
        public abstract bool Matches(string Hostname);
    }
    public class ResponseMatchedEventArgs : EventArgs
    {
        public required ParsedResponseDataEventArgs ParsedResponseData;
        public required string AddressListName;
        public required string[] MatchedNames;
        public required string[] Aliases;
        public required IPAddress[] Addresses;
    }
}