namespace DNS2TIK
{
    public class Program
    {
        public static Listener Listener = new();
        public static void Main(string[] args)
        {
            Listener.RecievedData += Listener_RecievedData;
            Console.ReadLine();
        }

        private static void Listener_RecievedData(object? sender, RecievedDataEventArgs e)
        {
            Console.WriteLine(BitConverter.ToString(e.Bytes));
        }
    }
}