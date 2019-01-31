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

        Task<IEnumerable<MusicCollectionEntry>> GetTracksAsync(int pageSize, string path);

        Task<Option<MusicCollectionEntry, Error>> UploadTrack(Track track, string path);
    }
}