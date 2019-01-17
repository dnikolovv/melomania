using System.Collections.Generic;
using System.Threading.Tasks;

namespace Melomania.Music
{
    public interface IMusicCollection
    {
        Task<IEnumerable<MusicCollectionEntry>> GetTracksAsync(int pageSize, string path);
        
        Task<UploadTrackResult> UploadTrack(Track track, string path);
    }
}
