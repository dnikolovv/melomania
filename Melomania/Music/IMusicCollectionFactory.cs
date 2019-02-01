using Optional;

namespace Melomania.Music
{
    public interface IMusicCollectionFactory
    {
        Option<IMusicCollection, Error> GetMusicCollection();
    }
}
