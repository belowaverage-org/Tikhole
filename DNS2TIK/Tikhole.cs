﻿using System.Reflection;

namespace Tikhole
{
    public class Tikhole
    {
        public static Assembly Assembly = Assembly.GetExecutingAssembly();
        public static Listener Listener;
        public static Forwarder Forwarder;
        public static Responder Responder;
        public static Parser Parser;
        public static Matcher Matcher;
        public static Committer Committer;
        public static void Main()
        {
            Logger.Info("Starting " + Assembly.GetCustomAttribute<AssemblyProductAttribute>()?.Product + " v" + Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion);
            Listener = new Listener();
            Forwarder = new Forwarder();
            Responder = new Responder();
            Parser = new Parser();
            Matcher = new Matcher();
            Committer = new Committer();
            Thread.Sleep(-1);
        }
    }
}