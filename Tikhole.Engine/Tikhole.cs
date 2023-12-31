using System.Reflection;

namespace Tikhole.Engine
{
    public class Tikhole
    {
        public static Assembly Assembly = Assembly.GetExecutingAssembly();
        public static Configurator? Configurator;
        public static Listener? Listener;
        public static Forwarder? Forwarder;
        public static Parser? Parser;
        public static Matcher? Matcher;
        public static Committer? Committer;
        public static Responder? Responder;
        public static void Main()
        {
            Logger.Info(Assembly.GetCustomAttribute<AssemblyProductAttribute>()?.Product + " v" + Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion);
            Configurator = new Configurator();
            Configurator.LoadConfig();
            Configurator.SaveConfig();
            Listener = new Listener();
            Forwarder = new Forwarder();
            Parser = new Parser();
            Matcher = new Matcher();
            Committer = new Committer();
            Responder = new Responder();
            Thread.Sleep(-1);
        }
    }
}