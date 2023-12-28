namespace DNS2TIK
{
    public static class Logger
    {
        public static void Success(string Message)
        {
            Raw(Message);
        }
        public static void Error(string Message) 
        {
            Raw(Message);
        }
        public static void Warning(string Message) 
        {
            Raw(Message);
        }
        public static void Info(string Message)
        {
            Raw(Message);
        }
        public static void Verbose(string Message) 
        {
            Raw(Message);
        }
        public static void Raw(string Message, bool WithDateTime = true)
        {
            if (WithDateTime) Console.Write(DateTime.Now + ": ");
            Console.WriteLine(Message);
        }
    }
}