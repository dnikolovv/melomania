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

        /// <summary>
        /// Retrieves an mp3 track from a given url.
        /// </summary>
        /// <param name="url">The url. (e.g. youtube)</param>
        /// <returns>A track or an error.</returns>
        Task<Option<Track, Error>> ExtractTrackFromUrl(string url);
    }
}