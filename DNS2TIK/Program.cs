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
            Listener.RecievedRequestData += Listener_RecievedRequestData;
            Forwarder.RecievedResponseData += Forwarder_RecievedResponseData;
            Parser.ParsedResponseData += Parser_ParsedResponseData;
            Console.ReadLine();
        }

        private static void Listener_RecievedRequestData(object? sender, RecievedRequestDataEventArgs e)
        {
            //Console.WriteLine(e.IPEndPoint.Address.ToString() + ':' + e.IPEndPoint.Port.ToString());
            //Console.WriteLine("Request: " + BitConverter.ToString(e.Bytes));
        }

        private static void Forwarder_RecievedResponseData(object? sender, RecievedResponseDataEventArgs e)
        {
            //Console.WriteLine("Response: " + BitConverter.ToString(e.Bytes));
        }
        private static void Parser_ParsedResponseData(object? sender, ParsedResponseDataEventArgs e)
        {
            Console.WriteLine("Questions: " + e.DNSPacket.Questions.Length);

            foreach (DNSQuestionRecord question in e.DNSPacket.Questions)
            {
                Console.WriteLine("Name: " + question.Name);
                Console.WriteLine("Type: " + question.Type.ToString());
                Console.WriteLine("Class: " + question.Class.ToString());
            }

            Console.WriteLine("Answers: " + e.DNSPacket.Answers.Length);

            foreach (DNSResourceRecord answer in e.DNSPacket.Answers)
            {
                Console.WriteLine("Name: " + answer.Name);
                Console.WriteLine("Type: " + answer.Type.ToString());
                Console.WriteLine("Class: " + answer.Class.ToString());
                Console.WriteLine("TTL: " + answer.TimeToLive);
                if (answer.Type == DNSType.A || answer.Type == DNSType.AAAA)
                {
                    foreach (IPAddress address in answer.ToAddresses())
                    {
                        Console.WriteLine("Address: " + address.ToString());
                    }
                }
            }

            Console.WriteLine();
            
        }
    }
}