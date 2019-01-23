using Optional;
using System.IO;
using System.Threading.Tasks;

namespace Melomania.Extractor
{
    public class TrackExtractor : ITrackExtractor
    {
        public Task<Option<Stream, Error>> ExtractTrackFromUrl(string url)
        {
            throw new System.NotImplementedException();
        }
    }
}
