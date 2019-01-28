using Melomania.Extractor;
using Melomania.Logging;
using Melomania.Music;
using Melomania.Utils;
using Optional;
using Optional.Async;
using System;
using System.Threading.Tasks;

namespace Melomania.CLI
{
    public class UploadFromUrlCommandHandler : AsyncCommand<UploadFromUrlArguments, UploadSuccessResult>
    {
        public UploadFromUrlCommandHandler(ITrackExtractor trackExtractor, IMusicCollection musicCollection, ILogger logger)
            : base(logger)
        {
            _trackExtractor = trackExtractor;
            _musicCollection = musicCollection;
        }

        private readonly ITrackExtractor _trackExtractor;
        private readonly IMusicCollection _musicCollection;

        public override Task<Option<UploadSuccessResult, Error>> ExecuteAsync(UploadFromUrlArguments arguments) =>
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
                        new UploadSuccessResult
                        {
                            FileName = result.Name,
                            Path = args.DestinationInCollection
                        });
                }));

        private Task<Option<Track, Error>> ExtractTrackFromUrl(string url) =>
            _trackExtractor.ExtractTrackFromUrl(url);

        private static bool IsValidUrl(string url) =>
            Uri.TryCreate(url, UriKind.Absolute, out Uri uriResult) &&
            (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }
}
