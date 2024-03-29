﻿using System.Net;
using System.Text.RegularExpressions;

namespace Tikhole.Engine
{
    public class Matcher : IDisposable
    {
        public static uint Matches = 0;
        public static Rules Rules = new();
        public event EventHandler<ResponseMatchedEventArgs>? ResponseMatched;
        public event EventHandler<ParsedResponseDataEventArgs>? MatchesMatchedAndOrCommitted;
        public Matcher()
        {
            Logger.Info("Starting Matcher...");
            if (Tikhole.Parser != null) Tikhole.Parser.ParsedResponseData += Parser_ParsedResponseData;
        }
        public void Dispose()
        {
            Matches = 0;
            Rules = new();
            Logger.Info("Matcher stopped.");
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
                    if (!rule.Enabled) continue;
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
                                string data = e.RecievedResponseData.Data.Span.ToLabelsString(ref index);
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
    [Rule("Host File", "A file in UNIX hosts file format that Tikhole will use to match domain names against.")]
    public class RuleHashSetDownloadableHostFile : RuleHashSetDownloadable
    {
        public RuleHashSetDownloadableHostFile(string Name, bool Enabled, Uri Uri, System.Timers.Timer UpdateTimer) : base(Name, Enabled, Uri, UpdateTimer) { }
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
        [RuleField("File Download", 2, "The file's URL to download from.")]
        public Uri Uri;
        [RuleField("Update Timer", 3, "The interval in seconds in which Tikhole will re-download the file.")]
        public System.Timers.Timer UpdateTimer;
        private protected static HttpClient HttpClient = new();
        public RuleHashSetDownloadable(string Name, bool Enabled, Uri Uri, System.Timers.Timer UpdateTimer) : base(Name, Enabled)
        {
            this.Uri = Uri;
            this.UpdateTimer = UpdateTimer;
            UpdateTimer.Elapsed += UpdateList;
            UpdateTimer.Enabled = true;
            UpdateTimer.Start();
            UpdateList();
        }
        public override void Dispose()
        {
            UpdateTimer.Stop();
            UpdateTimer.Dispose();
        }
        public virtual void UpdateList(object? a = null, object? b = null)
        {
            List.Clear();
        }
        public override bool Matches(string Hostname)
        {
            return List.Contains(Hostname);
        }
    }
    [Rule("Regular Expression", "Rule based off of a specified regular expression.")]
    public class RuleRegex : Rule
    {
        [RuleField("Match", 2, "The regular expression string that Tikhole will use to match domain names.")]
        public Regex Regex;
        public RuleRegex(string Name, bool Enabled, Regex Regex) : base(Name, Enabled)
        {
            this.Regex = Regex;
        }
        public override void Dispose() { }
        public override bool Matches(string Hostname)
        {
            return Regex.IsMatch(Hostname);
        }
    }
    public abstract class RuleHashSet : Rule
    {
        protected RuleHashSet(string Name, bool Enabled) : base(Name, Enabled) { }
        public HashSet<string> List = new();
    }
    public abstract class Rule : IDisposable
    {
        [RuleField("Name", 1, "The name of rule and IP list.")]
        public string Name;
        [RuleField("Enabled", 0)]
        public bool Enabled = true;
        public Rule(string Name, bool Enabled)
        {
            this.Name = Name;
            this.Enabled = Enabled;
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
    [AttributeUsage(AttributeTargets.Class)]
    public class RuleAttribute : Attribute
    {
        public string Name;
        public string? Hint;
        public RuleAttribute(string Name, string? Hint = null)
        {
            this.Name = Name;
            this.Hint = Hint;
        }
    }
    [AttributeUsage(AttributeTargets.Field)]
    public class RuleFieldAttribute : Attribute
    {
        public string Name;
        public uint Order;
        public string? Hint;
        public RuleFieldAttribute(string Name, uint Order = 0, string? Hint = null)
        {
            this.Name = Name;
            this.Order = Order;
            this.Hint = Hint;
        }
    }
}