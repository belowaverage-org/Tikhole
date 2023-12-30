namespace Tikhole.Engine
{
    public static class Logger
    {
        public static bool VerboseMode = false;
        private static SemaphoreSlim Semaphore = new(1, 1);
        public static void Success(string Message)
        {
            Raw("S: " + Message, ConsoleColor.Green);
        }
        public static void Error(string Message) 
        {
            Raw("E: " + Message, ConsoleColor.Red);
        }
        public static void Warning(string Message) 
        {
            Raw("W: " + Message, ConsoleColor.Yellow);
        }
        public static void Info(string Message)
        {
            Raw("I: " + Message, ConsoleColor.White);
        }
        public static void Verbose(string Message) 
        {
            Raw("V: " + Message);
        }
        public static void Raw(string Message, ConsoleColor Color = ConsoleColor.DarkGray, bool IncludePrefix = true)
        {
            Semaphore.Wait();
            Console.ForegroundColor = Color;
            if (IncludePrefix) Console.Write(DateTime.Now.ToString("HH:mm:ss.ff") + ": " + Thread.CurrentThread.ManagedThreadId + ": ");
            Console.WriteLine(Message);
            Semaphore.Release();
        }
    }
}