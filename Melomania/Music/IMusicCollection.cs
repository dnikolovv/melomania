using Melomania.Cloud.Results;
using Optional;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Melomania.Music
{
    public interface IMusicCollection
    {
        event Action<UploadFailureResult> OnUploadFailure;

        event Action<UploadProgress> OnUploadProgressChanged;

        event Action<UploadStarting> OnUploadStarting;

        event Action<UploadSuccessResult> OnUploadSuccessfull;

        /// <summary>
        /// Retrieves a list of tracks from a music collection.
        /// </summary>
        /// <param name="pageSize">The page size.</param>
        /// <param name="relativePath">A path relative to the collection root path.</param>
        /// <returns>A collection of music entries.</returns>
        Task<IEnumerable<MusicCollectionEntry>> GetTracksAsync(int pageSize, string path);

        /// <summary>
        /// Uploads a track to your music collection.
        /// </summary>
        /// <param name="track">The track.</param>
        /// <param name="relativePath">The path relative to your music collection root.</param>
        /// <returns>The new entry or an error.</returns>
        Task<Option<MusicCollectionEntry, Error>> UploadTrackAsync(Track track, string path);
    }
}