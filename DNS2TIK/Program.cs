using System.Net;

namespace DNS2TIK
{
    public class Program
    {
        public static Listener Listener;
        public static Forwarder Forwarder;
        public static Responder Responder;
        public static Parser Parser;
        public static Matcher Matcher;
        public static Committer Committer;
        public static void Main(string[] args)
        {
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