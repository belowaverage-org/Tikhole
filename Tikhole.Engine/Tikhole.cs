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
            Committer.TotalInstances = 0;
            Committers = new Committer[Committer.NeededInstances];
            for (int i = 0; i < Committer.NeededInstances; i++) Committers[i] = new Committer();
            new Responder();
        }
        public void Dispose()
        {
            Listener?.Dispose();
            if (Committers != null) foreach (Committer c in Committers) c.Dispose();
        }
    }
}