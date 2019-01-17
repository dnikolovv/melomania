using System.IO;

namespace Melomania.Music
{
    public class Track
    {
        public string Name { get; set; }

        public Stream Contents { get; set; }
    }
}