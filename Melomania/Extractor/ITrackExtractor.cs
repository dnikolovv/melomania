using Optional;
using System.IO;
using System.Threading.Tasks;

namespace Melomania.Extractor
{
    public interface ITrackExtractor
    {
        Task<Option<Stream, Error>> ExtractTrackFromUrl(string url);
    }
}
