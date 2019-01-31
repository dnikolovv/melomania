using Melomania.Music;
using Optional;
using System;
using System.Threading.Tasks;

namespace Melomania.Extractor
{
    public interface ITrackExtractor
    {
        event Action<TrackDownloadInfo> OnDownloadFinished;

        event Action<TrackDownloadInfo> OnDownloadProgressChanged;

        event Action<TrackDownloadInfo> OnDownloadStarting;

        Task<Option<Track, Error>> ExtractTrackFromUrl(string url);
    }
}