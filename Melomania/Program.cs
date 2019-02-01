﻿using Google.Apis.Auth.OAuth2;
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

            //args = new[] { "upload", "url", "https://www.youtube.com/watch?v=5N2_eWruhbM", "." };
            args = new[] { "setup" };

            var logger = new ConsoleLogger();
            var reader = new ConsoleReader();
            var toolsProvider = new YoutubeDlProvider();
            var trackExtractor = new YoutubeDlTrackExtractor(Configuration.ToolsFolder, Configuration.TempFolder);
            var configuration = new Configuration();

            // TODO: Abstract over the google drive service to enable implementing other cloud storage providers
            var driveService = await GetGoogleDriveService();
            var musicCollectionFactory = new GoogleDriveMusicCollectionFactory(driveService, configuration);

            trackExtractor.OnExtractionStarting += info => logger.WriteLine($"Now extracting '{info.Title}'...");
            trackExtractor.OnExtractionProgressChanged += info => logger.WriteLine($"'{info.Title}' progress: {info.Progress}%");
            trackExtractor.OnExtractionFinished += info => logger.WriteLine($"Successfully extracted '{info.Title}'!");

            toolsProvider.OnToolDownloadStarting += tool => logger.WriteLine($"Downloading '{tool.Name}'...");
            toolsProvider.OnToolDownloadCompleted += tool => logger.WriteLine($"Successfully downloaded '{tool.Name}'!");

            driveService.OnUploadStarting += info => logger.WriteLine($"Now uploading '{info.FileName}' into '{info.DestinationPath}'...");
            driveService.OnUploadProgressChanged += info => logger.WriteLine($"'{info.FileName}' upload progress: {info.Percentage}%");
            driveService.OnUploadSuccessfull += info => logger.WriteLine($"Successfully uploaded '{info.FileName}'!");
            driveService.OnUploadFailure += info => logger.WriteLine($"Failed to upload '{info.FileName}' :(");

            // TODO: Implement a command for listing the collection's contents
            // TODO: Implement a command to redownload the tools if for some reason they became corrupted

            var ioHandler = new IOHandler(
                logger,
                reader,
                toolsProvider,
                musicCollectionFactory,
                trackExtractor,
                driveService,
                configuration);

            var executionResult = ioHandler.CheckWhetherToolsAreDownloaded(Configuration.ToolsFolder).FlatMapAsync(_ =>
                                  ioHandler.HandleArguments(args));

            (await executionResult).Match(
                some: _ => Console.Write(string.Empty),
                none: error => Console.WriteLine(error));
        }

        /// <summary>
        /// Implicitly handles authenticating to Google as their API requires it.
        /// </summary>
        /// <returns>A google drive service instance.</returns>
        private static async Task<GoogleDriveService> GetGoogleDriveService()
        {
            // TODO: Use embedded resources for the credentials
            using (var stream = new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
            {
                var clientSecrets = GoogleClientSecrets.Load(stream).Secrets;

                var driveService = await new GoogleDriveServiceFactory()
                    .GetDriveService(clientSecrets);

                return driveService;
            }
        }
    }
}