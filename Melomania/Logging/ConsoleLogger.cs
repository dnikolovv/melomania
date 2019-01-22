using System;

namespace Melomania.Logging
{
    public class ConsoleLogger : ILogger
    {
        public void Write(string message) => Console.Write(message);

        public void WriteLine(string message) => Console.WriteLine(message);
    }
}
