using Melomania.Music;
using NYoutubeDL.Models;
using Optional;
using System;
using System.Threading.Tasks;

namespace Melomania.Extractor
{
    public interface ITrackExtractor
    {
        // TODO: This shouldn't use the DownloadInfo from the YoutubeDl library
        event Action<DownloadInfo> OnDownloadStarting;
        event Action<int> OnDownloadProgressChanged;
        event Action<DownloadInfo> OnDownloadFinished;

        Task<Option<Track, Error>> ExtractTrackFromUrl(string url);
    }
}
