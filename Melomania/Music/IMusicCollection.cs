using Optional;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Melomania.Music
{
    public interface IMusicCollection
    {
        event Action<UploadStarting> OnUploadStarting;

        event Action<UploadProgress> OnUploadProgressChanged;

        event Action<UploadSuccessResult> OnUploadSuccessfull;

        event Action<UploadFailureResult> OnUploadFailure;

        Task<IEnumerable<MusicCollectionEntry>> GetTracksAsync(int pageSize, string path);

        Task<Option<MusicCollectionEntry, Error>> UploadTrack(Track track, string path);
    }
}
