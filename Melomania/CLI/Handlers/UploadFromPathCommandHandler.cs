﻿using Melomania.CLI.Arguments;
using Melomania.CLI.Results;
using Melomania.Music;
using Optional;
using Optional.Async;
using System.IO;
using System.Threading.Tasks;

namespace Melomania.CLI.Handlers
{
    public class UploadFromPathCommandHandler : IAsyncCommandHandler<UploadFromPathArguments, UploadSuccessfulResult>
    {
        private readonly IMusicCollection _musicCollection;

        public UploadFromPathCommandHandler(IMusicCollection musicCollection)
        {
            _musicCollection = musicCollection;
        }

        public Task<Option<UploadSuccessfulResult, Error>> ExecuteAsync(UploadFromPathArguments arguments) =>
            arguments
                .SomeNotNull<UploadFromPathArguments, Error>("You must provide non-null arguments.")
                .Filter(args => !string.IsNullOrEmpty(args.FilePath) && File.Exists(args.FilePath), $"No files were found at path '{arguments.FilePath}'")
                .Filter(args => !string.IsNullOrEmpty(args.FileName), $"You must provide a non-null file name.")
                .Filter(args => !string.IsNullOrEmpty(args.DestinationInCollection), $"You must provide a valid path inside your collection (use '.' for root)")
                .FlatMapAsync(async args =>
                {
                    using (var trackStream = File.OpenRead(args.FilePath))
                    {
                        var trackToUpload = new Track
                        {
                            Contents = trackStream,
                            Name = args.FileName
                        };

                        var uploadResult = await _musicCollection.UploadTrackAsync(trackToUpload, path: args.DestinationInCollection);

                        return uploadResult.Map(result =>
                            new UploadSuccessfulResult
                            {
                                FileName = result.Name,
                                Path = args.DestinationInCollection
                            });
                    }
                });
    }
}