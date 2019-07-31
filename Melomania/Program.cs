using Google.Apis.Auth.OAuth2;
using Melomania.Cloud.GoogleDrive;
using Melomania.Config;
using Melomania.Extractor;
using Melomania.IO;
using Melomania.Music.GoogleDrive;
using Melomania.Tools;
using Optional.Async;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Melomania
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            Console.CursorVisible = false;

            // For testing purposes
            //args = new[] { "upload", "url", "https://www.youtube.com/watch?v=pbwlKkR9-Ek", ".", "Plamen, Paul Stanga, Sasho Velkov i Marian Marikanu Varna 3" };
            //args = new[] { "setup", "." };

            var logger = new ConsoleLogger();
            var reader = new ConsoleReader();
            var configuration = new Configuration();
            var toolsProvider = new YoutubeDlProvider();
            var trackExtractor = new YoutubeDlTrackExtractor(configuration.ToolsFolder, configuration.TempFolder);
            var driveService = await GetGoogleDriveService(configuration);
            var musicCollectionFactory = new GoogleDriveMusicCollectionFactory(driveService, configuration);

            trackExtractor.OnExtractionStarting += info => logger.WriteLine($"Extracting '{info.Title}'...");
            trackExtractor.OnExtractionProgressChanged += info => logger.WriteLine($"'{info.Title}' progress: {info.Progress}%");
            trackExtractor.OnExtractionFinished += info => logger.WriteLine($"Successfully extracted '{info.Title}'!");

            toolsProvider.OnToolDownloadStarting += tool => logger.WriteLine($"Downloading '{tool.Name}'...");
            toolsProvider.OnToolDownloadCompleted += tool => logger.WriteLine($"Successfully downloaded '{tool.Name}'!");

            driveService.OnUploadStarting += info => logger.WriteLine($"Uploading '{info.FileName}' into '{info.DestinationPath}'...");
            driveService.OnUploadProgressChanged += info => logger.WriteLine($"'{info.FileName}' upload progress: {info.Percentage:F1}%");
            driveService.OnUploadSuccessfull += info => logger.WriteLine($"Successfully uploaded '{info.FileName}' into '{info.Path}'!");
            driveService.OnUploadFailure += info => logger.WriteLine($"Failed to upload '{info.FileName}' :(");

            var ioHandler = new IOHandler(
                logger,
                reader,
                toolsProvider,
                musicCollectionFactory,
                trackExtractor,
                driveService,
                configuration);

            var executionResult = ioHandler.DownloadTools().FlatMapAsync(_ =>
                                  ioHandler.HandleArguments(args));

            (await executionResult).MatchNone(
                error => Console.WriteLine(error));
        }

        /// <summary>
        /// Implicitly handles authenticating to Google as their API requires it.
        /// </summary>
        /// <returns>A google drive service instance.</returns>
        private static async Task<GoogleDriveService> GetGoogleDriveService(Configuration configuration)
        {
            using (var stream = new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
            {
                var clientSecrets = GoogleClientSecrets.Load(stream).Secrets;

                var driveService = await new GoogleDriveServiceFactory()
                    .GetDriveService(clientSecrets, configuration);

                return driveService;
            }
        }
    }
}