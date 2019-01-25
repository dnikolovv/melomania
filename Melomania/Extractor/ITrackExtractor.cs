using Melomania.Music;
using Optional;
using System.Threading.Tasks;

namespace Melomania.Extractor
{
    public interface ITrackExtractor
    {
        Task<Option<Track, Error>> ExtractTrackFromUrl(string url, string fileName);
    }
}
