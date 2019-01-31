using System;

namespace Melomania.IO
{
    public class ConsoleLogger : ILogger
    {
        public void Write(string message) => Console.Write(message);

        public void WriteLine(string message = null) => Console.WriteLine(message ?? string.Empty);
    }
}