using Melomania.CLI.Arguments;
using Melomania.CLI.Handlers;
using Melomania.Cloud.GoogleDrive;
using Melomania.Config;
using Melomania.Extractor;
using Melomania.IO;
using Melomania.Music;
using Melomania.Tools;
using Optional;
using Optional.Async;
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
                "setup",
                "upload path {folder path} {path inside collection ('.' for root)}",
                "upload url {url} {path inside collectio ('.' for root)} {*optional* custom file name}"
            };

        public Task<Option<DownloadToolsResult, Error>> CheckWhetherToolsAreDownloaded(string toolsFolder) =>
            _toolsProvider
                .DownloadTools(toolsFolder, ignoreIfExisting: true);

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

                case "upload":
                    return await Upload(args.Skip(1).ToArray());

                default:
                    return CommandNotSupported();
            }
        }

        private Option<Unit, Error> CommandNotSupported() =>
            Option.None<Unit, Error>(new string[] { "Supported commands:" }.Concat(SupportedCommands).ToArray());

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