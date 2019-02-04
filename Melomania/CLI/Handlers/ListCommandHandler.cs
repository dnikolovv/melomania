using Melomania.CLI.Arguments;
using Melomania.CLI.Results;
using Melomania.Music;
using Optional;
using Optional.Async;
using System.Threading.Tasks;

namespace Melomania.CLI.Handlers
{
    public class ListCommandHandler : IAsyncCommandHandler<ListArguments, CollectionList>
    {
        public ListCommandHandler(IMusicCollection musicCollection)
        {
            _musicCollection = musicCollection;
        }

        private readonly IMusicCollection _musicCollection;

        public Task<Option<CollectionList, Error>> ExecuteAsync(ListArguments arguments) =>
            arguments
                .SomeNotNull<ListArguments, Error>("You must provide non-null arguments.")
                .Filter(args => !string.IsNullOrEmpty(args.Path), "You must provide a non-empty path.")
                .Filter(args => args.PageSize > 0, "The page size must be larger than 0.")
                .FlatMapAsync(args =>
                    _musicCollection
                            .GetEntriesAsync(args.PageSize, args.Path)
                            .MapAsync(async tracks => new CollectionList
                            {
                                Entries = tracks
                            }));
    }
}
