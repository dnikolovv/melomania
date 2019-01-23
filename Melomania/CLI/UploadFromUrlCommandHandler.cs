using Melomania.Extractor;
using Melomania.Logging;
using Melomania.Music;
using Optional;
using Optional.Async;
using System;
using System.IO;
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
                .Filter(args => !string.IsNullOrEmpty(args.FileName), "You must provide a non-null file name.")
                .Filter(args => !string.IsNullOrEmpty(args.DestinationInCollection), $"You must provide a valid path inside your collection (use '.' for root)")
                .Filter(args => IsValidUrl(args.Url), "Invalid url.").FlatMapAsync(args =>
                 ExtractTrackFromUrl(args.Url).MapAsync(async stream => new Track { Contents = stream, Name = args.FileName })
                .FlatMapAsync(async track =>
                {
                    var uploadResult = await _musicCollection.UploadTrack(track, arguments.DestinationInCollection);
                    
                    return uploadResult.Map(result =>
                        new UploadSuccessResult
                        {
                            FileName = result.Name,
                            Path = args.DestinationInCollection
                        });
                }));

        private Task<Option<Stream, Error>> ExtractTrackFromUrl(string url) =>
            _trackExtractor.ExtractTrackFromUrl(url);

        private static bool IsValidUrl(string url) =>
            Uri.TryCreate(url, UriKind.Absolute, out Uri uriResult) &&
            (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

        //Stream trackStream = ExtractTrackStreamFromUrl(arguments.Url);

        //var track = new Track
        //{
        //    Contents = trackStream,
        //    Name = arguments.FileName
        //};

        //var result = _musicCollection.UploadTrack(track, arguments.DestinationInCollection);

        //using (var trackStream = File.OpenRead(args.FilePath))
        //            {
        //                var trackToUpload = new Track
        //                {
        //                    Contents = trackStream,
        //                    Name = args.FileName
        //                };

        //                var uploadResult = await _musicCollection.UploadTrack(trackToUpload, path: args.DestinationInCollection);

        //                return uploadResult.Map(result =>
        //                    new UploadSuccessResult
        //                    {
        //                        FileName = result.Name,
        //                        Path = args.DestinationInCollection
        //                    });
        //            }
    }
}
