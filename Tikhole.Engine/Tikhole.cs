using System.Reflection;

namespace Tikhole.Engine
{
    public class Tikhole
    {
        public static Assembly Assembly = Assembly.GetExecutingAssembly();
        public static Listener? Listener;
        public static Forwarder? Forwarder;
        public static Parser? Parser;
        public static Matcher? Matcher;
        public static void Main()
        {
            new Tikhole();
            Thread.Sleep(-1);
        }
        public Tikhole()
        {
            Logger.Info(Assembly.GetCustomAttribute<AssemblyProductAttribute>()?.Product + " v" + Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion);
            ThreadPool.SetMaxThreads(int.MaxValue, int.MaxValue);
            Configurator.LoadConfig();
            Configurator.SaveConfig();
            Listener = new Listener();
            Forwarder = new Forwarder();
            Parser = new Parser();
            Matcher = new Matcher();
            for (int i = 0; i < Committer.NeededInstances; i++) new Committer();
            new Responder();
        }
    }
}