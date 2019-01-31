using Melomania.Music;
using Optional;
using System;
using System.Threading.Tasks;

namespace Melomania.Extractor
{
    public interface ITrackExtractor
    {
        event Action<TrackExtractionInfo> OnExtractionFinished;

        event Action<TrackExtractionInfo> OnExtractionProgressChanged;

        event Action<TrackExtractionInfo> OnExtractionStarting;

        Task<Option<Track, Error>> ExtractTrackFromUrl(string url);
    }
}