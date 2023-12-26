namespace DNS2TIK
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Listener Listener = new();
            Listener.RecievedData += Listener_RecievedData;
            Console.ReadLine();
        }

        private static void Listener_RecievedData(object? sender, EventArgs e)
        {
            Console.WriteLine("Got data");
        }
    }
}