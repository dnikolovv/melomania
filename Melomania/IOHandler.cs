using Melomania.CLI.Arguments;
using Melomania.CLI.Handlers;
using Melomania.Cloud.GoogleDrive;
using Melomania.Config;
using Melomania.Extractor;
using Melomania.IO;
using Melomania.Music;
using Melomania.Tools;
using Melomania.Utils;
using Optional;
using Optional.Async;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Melomania
{
    public class IOHandler
    {
        private readonly Configuration _configuration;

        private readonly GoogleDriveService _googleDriveService;

        private readonly ILogger _logger;

        private readonly IMusicCollectionFactory _musicCollectionFactory;

        private readonly IReader _reader;

        private readonly IToolsProvider _toolsProvider;

        private readonly ITrackExtractor _trackExtractor;

        public IOHandler(
            ILogger logger,
            IReader reader,
            IToolsProvider toolsProvider,
            IMusicCollectionFactory musicCollectionFactory,
            ITrackExtractor trackExtractor,
            GoogleDriveService googleDriveService,
            Configuration configuration)
        {
            _logger = logger;
            _reader = reader;
            _toolsProvider = toolsProvider;
            _musicCollectionFactory = musicCollectionFactory;
            _trackExtractor = trackExtractor;
            _googleDriveService = googleDriveService;
            _configuration = configuration;
        }

        public static string[] SupportedCommands =>
            new[]
            {
                "melomania setup",
                "melomania download-tools",
                "melomania list <collection-folder-path> ('.' for root)",
                "melomania upload path <path> <collection-folder-path> ('.' for root)",
                "melomania upload url <url> <collection-folder-path> ('.' for root) <[optional-custom-filename]>"
            };

        public Task<Option<Unit, Error>> DownloadTools(bool ignoreIfExisting = true) =>
            _toolsProvider
                .DownloadTools(Configuration.ToolsFolder, ignoreIfExisting);

        public async Task<Option<Unit, Error>> HandleArguments(string[] args)
        {
            // This is indeed a very lame way of parsing the command line parameters, but it allows us to achieve
            // multi-level verbs (e.g. "upload path ...") and async handlers fairly easily. I couldn't find an arguments parser
            // library that could do this without too much code gymnastics
            var command = args.FirstOrDefault();

            switch (command)
            {
                case "setup":
                    return await Setup();

                case "download-tools":
                    return await DownloadTools(ignoreIfExisting: false);

                case "list":
                    return await List(args.Skip(1).ToArray());

                case "upload":
                    return await Upload(args.Skip(1).ToArray());

                default:
                    return CommandNotSupported();
            }
        }

        private Option<Unit, Error> CommandNotSupported() =>
            Option.None<Unit, Error>(new string[] { "Usage:" }.Concat(SupportedCommands).ToArray());

        private Task<Option<Unit, Error>> List(string[] args)
        {
            var path = args.ElementAtOrDefault(0);

            return path
                .SomeNotNull((Error)"You must provide a valid path (use '.' for root)").FlatMapAsync(_ =>
                _musicCollectionFactory
                    .GetMusicCollection()
                    .FlatMapAsync(async musicCollection =>
                    {
                        _logger.WriteLine("Fetching collection contents...");

                        var handler = new ListCommandHandler(musicCollection);

                        var result = await handler.ExecuteAsync(new ListArguments
                        {
                            Path = path
                        });

                        return result.Map(collection =>
                        {
                            RenderEntries(collection.Entries);
                            return Unit.Value;
                        });
                    }));
        }

        private void RenderEntries(IEnumerable<MusicCollectionEntry> entries)
        {
            var groupedByTypes = entries
                .GroupBy(e => e.Type);

            foreach (var group in groupedByTypes)
            {
                _logger.WriteLine();
                _logger.WriteLine($"{group.Key.GetDescription()}:");

                foreach (var entry in group)
                {
                    _logger.WriteLine(entry.Name);
                }
            }
        }

        private async Task<Option<Unit, Error>> Setup()
        {
            var handler = new SetupCommandHandler(
                _configuration,
                _googleDriveService);

            _logger.Write("Enter your root music folder (e.g. 'Music' (paths are still not accepted)): ");
            var rootCollectionFolder = _reader.ReadLine();

            var result = await handler.ExecuteAsync(new SetupArguments
            {
                RootCollectionFolder = rootCollectionFolder
            });

            return await result.MapAsync(async _ =>
            {
                _logger.WriteLine($"Successfully set '{rootCollectionFolder}' as your base collection folder!");
                return Unit.Value;
            });
        }

        private async Task<Option<Unit, Error>> Upload(string[] args)
        {
            var subCommand = args.FirstOrDefault();

            switch (subCommand)
            {
                case "path":
                    return await UploadFromPath(args.Skip(1).ToArray());

                case "url":
                    return await UploadFromUrl(args.Skip(1).ToArray());

                default:
                    return CommandNotSupported();
            }
        }

        private Task<Option<Unit, Error>> UploadFromPath(string[] args) =>
            _musicCollectionFactory
                .GetMusicCollection()
                .FlatMapAsync(async musicCollection =>
                {
                    var handler = new UploadFromPathCommandHandler(musicCollection);

                    var filePath = args.ElementAtOrDefault(0);
                    var destinationFolder = args.ElementAtOrDefault(1);

                    var result = await handler.ExecuteAsync(new UploadFromPathArguments
                    {
                        FilePath = filePath,
                        FileName = Path.GetFileName(filePath),
                        DestinationInCollection = destinationFolder
                    });

                    return await result.MapAsync(async _ => Unit.Value);
                });

        private Task<Option<Unit, Error>> UploadFromUrl(string[] args) =>
            _musicCollectionFactory
                .GetMusicCollection()
                .FlatMapAsync(async musicCollection =>
                {
                    var handler = new UploadFromUrlCommandHandler(_trackExtractor, musicCollection);

                    var url = args.ElementAtOrDefault(0);
                    var destination = args.ElementAtOrDefault(1);
                    var fileName = args.ElementAtOrDefault(2);

                    var arguments = new UploadFromUrlArguments
                    {
                        Url = url,
                        DestinationInCollection = destination,
                        CustomFileName = fileName
                    };

                    var result = await handler.ExecuteAsync(arguments);

                    return await result.MapAsync(async _ => Unit.Value);
                });
    }
}