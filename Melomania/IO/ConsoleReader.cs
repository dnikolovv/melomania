using System;

namespace Melomania.IO
{
    public class ConsoleReader : IReader
    {
        public string ReadLine() =>
            Console.ReadLine();
    }
}
