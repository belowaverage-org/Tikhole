using System.Text.RegularExpressions;
using System.Xml;

namespace Tikhole
{
    public class Configurator
    {
        public static string ConfigFileName = "Tikhole.xml";
        public void LoadConfig()
        {
            try
            {
                Logger.Info("Reading config from " + ConfigFileName + "...");
                XmlDocument config = new();
                config.Load(ConfigFileName);
                config.ReadSetting("/Tikhole/RouterOS/IPListTimeout", ref Committer.ListTTL);
                config.ReadSetting("/Tikhole/RouterOS/ApiUserName", ref Committer.UserName);
                config.ReadSetting("/Tikhole/RouterOS/ApiPassword", ref Committer.Password);
                config.ReadSetting("/Tikhole/RouterOS/ApiEndpoint", ref Committer.RouterOSIPEndPoint);
                config.ReadSetting("/Tikhole/Forwarder/DnsEndpoint", ref Forwarder.DNSServer);
                config.ReadSetting("/Tikhole/Logger/VerboseMode", ref Logger.VerboseMode);
                XmlNode? rulesNode = config.SelectSingleNode("/Tikhole/Matcher/Rules");
                XmlNodeList? xRules = config.SelectNodes("/Tikhole/Matcher/Rules/*");
                if (xRules != null && rulesNode != null)
                {
                    MatchTable table = new();
                    foreach (XmlNode xRule in xRules) table.Add(xRule.Name, new Regex(xRule.InnerText));
                    Matcher.MatchTable = table;
                }
                Logger.Success("Config " + ConfigFileName + " read.");
            }
            catch
            {
                Logger.Error("Could not read config at " + ConfigFileName + ".");
            }
        }
        public void SaveConfig()
        {
            try
            {
                Logger.Info("Writing config to " + ConfigFileName + ".");
                XmlDocument config = new();
                XmlNode? root = config.AppendChild(config.CreateElement("Tikhole"));
                XmlNode? routerOS = root?.AppendChild(config.CreateElement("RouterOS"));
                routerOS?.AddSetting("IPListTimeout", Committer.ListTTL);
                routerOS?.AddSetting("ApiUserName", Committer.UserName);
                routerOS?.AddSetting("ApiPassword", Committer.Password);
                routerOS?.AddSetting("ApiEndpoint", Committer.RouterOSIPEndPoint.ToString());
                XmlNode? forwarder = root?.AppendChild(config.CreateElement("Forwarder"));
                forwarder?.AddSetting("DnsEndpoint", Forwarder.DNSServer.ToString());
                XmlNode? logger = root?.AppendChild(config.CreateElement("Logger"));
                logger?.AddSetting("VerboseMode", Logger.VerboseMode.ToString());
                XmlNode? rules = root?.AppendChild(config.CreateElement("Matcher"))?.AppendChild(config.CreateElement("Rules"));
                foreach (KeyValuePair<string, Regex> rule in Matcher.MatchTable)
                {
                    XmlNode? node = rules?.AppendChild(config.CreateElement(rule.Key));
                    if (node != null) node.InnerText = rule.Value.ToString();
                }
                config.Save(ConfigFileName);
                Logger.Success("Config saved to " + ConfigFileName + ".");
            }
            catch
            {
                Logger.Error("Failed to save config to " + ConfigFileName + ".");
            }
        }
    }
}