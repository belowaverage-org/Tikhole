using System.Reflection;

namespace Tikhole.Engine
{
    public class Tikhole : IDisposable
    {
        public static Assembly Assembly = Assembly.GetExecutingAssembly();
        public static Listener? Listener;
        public static Forwarder? Forwarder;
        public static Parser? Parser;
        public static Matcher? Matcher;
        public static Syncer? Syncer;
        public static Responder? Responder;
        public static Committer[]? Committers;
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
            Syncer = new Syncer();
            Committer.TotalInstances = 0;
            Committers = new Committer[Committer.NeededInstances];
            for (int i = 0; i < Committer.NeededInstances; i++) Committers[i] = new Committer();
            Responder = new Responder();
        }
        public void Dispose()
        {
            foreach (Rule rule in Matcher.Rules) rule.Dispose();
            Listener?.Dispose(); 
            Listener = null;
            Forwarder?.Dispose();
            Forwarder = null;
            Parser?.Dispose();
            Parser = null;
            Matcher?.Dispose();
            Matcher = null;
            Syncer?.Dispose();
            Syncer = null;
            Responder?.Dispose();
            Responder = null;
            if (Committers != null) foreach (Committer c in Committers) c.Dispose();
            Committers = null;
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Aggressive | GCCollectionMode.Forced, true);
            Logger.Info(Assembly.GetCustomAttribute<AssemblyProductAttribute>()?.Product + " stopped.");
        }
    }
}