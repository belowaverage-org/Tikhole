using System.Xml;

namespace Tikhole.Engine
{
    public static class Configurator
    {
        public static string ConfigFilePath = "./config/";
        public static string ConfigFileName = ConfigFilePath + "Tikhole.xml";
        public static void LoadConfig()
        {
            try
            {
                Logger.Info("Reading config from " + ConfigFileName + "...");
                XmlDocument config = new();
                config.Load(ConfigFileName);
                config.ReadSetting("/Tikhole/RouterOS/IPListTimeout", ref Committer.ListTTL);
                config.ReadSetting("/Tikhole/RouterOS/IPListTimeoutUpdateDelay", ref Committer.ListTTLUpdateDelay);
                config.ReadSetting("/Tikhole/RouterOS/ApiUserName", ref Committer.UserName);
                config.ReadSetting("/Tikhole/RouterOS/ApiPassword", ref Committer.Password);
                config.ReadSetting("/Tikhole/RouterOS/ApiEndpoint", ref Committer.RouterOSIPEndPoint);
                config.ReadSetting("/Tikhole/RouterOS/ComitterDelayMS", ref Committer.ComitterDelayMS);
                config.ReadSetting("/Tikhole/RouterOS/SyncerIntervalSeconds", ref Syncer.SyncerIntervalSeconds);
                config.ReadSetting("/Tikhole/RouterOS/ApiEndpoint", ref Committer.RouterOSIPEndPoint);
                config.ReadSetting("/Tikhole/RouterOS/ApiConnections", ref Committer.NeededInstances);
                config.ReadSetting("/Tikhole/Forwarder/DnsEndpoint", ref Forwarder.DNSServer);
                config.ReadSetting("/Tikhole/Responder/WaitForMatcherAndCommitter", ref Responder.WaitForMatcherAndCommitter);
                config.ReadSetting("/Tikhole/Logger/VerboseMode", ref Logger.VerboseMode);
                config.ReadRules("/Tikhole/Matcher/Rules/*", ref Matcher.Rules);
                Logger.Success("Config " + ConfigFileName + " read.");
            }
            catch
            {
                Logger.Error("Could not read config at " + ConfigFileName + ".");
            }
        }
        public static void SaveConfig()
        {
            try
            {
                Logger.Info("Writing config to " + ConfigFileName + ".");
                XmlDocument config = new();
                XmlNode? root = config.AppendChild(config.CreateElement("Tikhole"));
                XmlNode? routerOS = root?.AppendChild(config.CreateElement("RouterOS"));
                routerOS?.AddSetting("IPListTimeout", Committer.ListTTL.ToString());
                routerOS?.AddSetting("IPListTimeoutUpdateDelay", Committer.ListTTLUpdateDelay.ToString());
                routerOS?.AddSetting("ApiUserName", Committer.UserName);
                routerOS?.AddSetting("ApiPassword", Committer.Password);
                routerOS?.AddSetting("ApiEndpoint", Committer.RouterOSIPEndPoint.ToString());
                routerOS?.AddSetting("ApiConnections", Committer.NeededInstances.ToString());
                routerOS?.AddSetting("ComitterDelayMS", Committer.ComitterDelayMS.ToString());
                routerOS?.AddSetting("SyncerIntervalSeconds", Syncer.SyncerIntervalSeconds.ToString());
                XmlNode? forwarder = root?.AppendChild(config.CreateElement("Forwarder"));
                forwarder?.AddSetting("DnsEndpoint", Forwarder.DNSServer.ToString());
                XmlNode? responder = root?.AppendChild(config.CreateElement("Responder"));
                responder?.AddSetting("WaitForMatcherAndCommitter", Responder.WaitForMatcherAndCommitter.ToString());
                XmlNode? logger = root?.AppendChild(config.CreateElement("Logger"));
                logger?.AddSetting("VerboseMode", Logger.VerboseMode.ToString());
                XmlNode? rules = root?.AppendChild(config.CreateElement("Matcher"))?.AppendChild(config.CreateElement("Rules"));
                rules?.AddRules(Matcher.Rules);
                if (!Directory.Exists(ConfigFilePath)) _ = Directory.CreateDirectory(ConfigFilePath);
                config.Save(ConfigFileName);
                Logger.Success("Config saved to " + ConfigFileName + ".");
            }
            catch (Exception e)
            {
                Logger.Error("Failed to save config to " + ConfigFileName + ".");
                throw new Exception(e.Message);
            }
        }
    }
}