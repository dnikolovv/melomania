using System.Collections.Generic;
using System.Threading.Tasks;

namespace Melomania
{
    public interface IMusicCollection
    {
        Task<IEnumerable<MusicCollectionEntry>> GetTracksAsync(int pageSize);
    }
}
