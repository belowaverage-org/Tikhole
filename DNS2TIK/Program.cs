using System.Net;

namespace DNS2TIK
{
    public class Program
    {
        public static Listener Listener;
        public static Forwarder Forwarder;
        public static Responder Responder;
        public static Parser Parser;
        public static void Main(string[] args)
        {
            Listener = new Listener();
            Forwarder = new Forwarder();
            Responder = new Responder();
            Parser = new Parser();
            Parser.ParsedResponseData += Parser_ParsedResponseData;
            Thread.Sleep(-1);
        }
        private static void Parser_ParsedResponseData(object? sender, ParsedResponseDataEventArgs e)
        {
            Console.WriteLine("Questions: " + e.DNSPacket.Questions.Length);
            foreach (DNSQuestionRecord question in e.DNSPacket.Questions)
            {
                Console.WriteLine("    Name: " + question.Name);
                Console.WriteLine("        Type: " + question.Type.ToString());
                Console.WriteLine("        Class: " + question.Class.ToString());
            }
            Console.WriteLine("Answers: " + e.DNSPacket.Answers.Length);
            foreach (DNSResourceRecord answer in e.DNSPacket.Answers)
            {
                Console.WriteLine("    Name: " + answer.Name);
                Console.WriteLine("        Type: " + answer.Type.ToString());
                Console.WriteLine("        Class: " + answer.Class.ToString());
                Console.WriteLine("        TTL: " + answer.TimeToLive);
                if (answer.Type == DNSType.CNAME)
                {
                    int index = 0;
                    //Console.WriteLine("        Data: " + answer.Data.ToLabelsString(ref index));
                }
                if (answer.Type == DNSType.A || answer.Type == DNSType.AAAA)
                {
                    foreach (IPAddress address in answer.ToAddresses())
                    {
                        Console.WriteLine("        Address: " + address.ToString());
                    }
                }
            }
            Console.WriteLine();
        }
    }
}