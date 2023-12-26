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
            Console.ReadLine();
        }

        private static void Listener_RecievedRequestData(object? sender, RecievedRequestDataEventArgs e)
        {
            Console.WriteLine(e.IPEndPoint.Address.ToString() + ':' + e.IPEndPoint.Port.ToString());
            Console.WriteLine("Request: " + BitConverter.ToString(e.Bytes));
        }

        private static void Forwarder_RecievedResponseData(object? sender, RecievedResponseDataEventArgs e)
        {
            Console.WriteLine("Response: " + BitConverter.ToString(e.Bytes));
        }
    }
}