using Melomania.CLI.Arguments;
using Melomania.CLI.Results;
using Melomania.Extractor;
using Melomania.Music;
using Melomania.Utils;
using Optional;
using Optional.Async;
using System;
using System.Threading.Tasks;

namespace Melomania.CLI.Handlers
{
    public class UploadFromUrlCommandHandler : IAsyncCommandHandler<UploadFromUrlArguments, UploadSuccessfulResult>
    {
        private readonly IMusicCollection _musicCollection;

        private readonly ITrackExtractor _trackExtractor;

        public UploadFromUrlCommandHandler(ITrackExtractor trackExtractor, IMusicCollection musicCollection)
        {
            _trackExtractor = trackExtractor;
            _musicCollection = musicCollection;
        }

        public Task<Option<UploadSuccessfulResult, Error>> ExecuteAsync(UploadFromUrlArguments arguments) =>
            arguments
                .SomeNotNull<UploadFromUrlArguments, Error>("You must provide non-null arguments.")
                .Filter(args => !string.IsNullOrEmpty(args.Url), "You must provide a non-null url.")
                .Filter(args => !string.IsNullOrEmpty(args.DestinationInCollection), $"You must provide a valid path inside your collection (use '.' for root)")
                .Filter(args => IsValidUrl(args.Url), "Invalid url.").FlatMapAsync(args =>
                 ExtractTrackFromUrl(args.Url)
                .FlatMapAsync(async track =>
                {
                    if (!string.IsNullOrEmpty(arguments.CustomFileName))
                    {
                        track.Name = arguments.CustomFileName.SetExtension("mp3");
                    }

                    var uploadResult = await _musicCollection.UploadTrack(track, arguments.DestinationInCollection);

                    return uploadResult.Map(result =>
                        new UploadSuccessfulResult
                        {
                            FileName = result.Name,
                            Path = args.DestinationInCollection
                        });
                }));

        private static bool IsValidUrl(string url) =>
            Uri.TryCreate(url, UriKind.Absolute, out Uri uriResult) &&
            (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

        private Task<Option<Track, Error>> ExtractTrackFromUrl(string url) =>
            _trackExtractor.ExtractTrackFromUrl(url);
    }
}