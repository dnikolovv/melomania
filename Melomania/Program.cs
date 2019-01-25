using Google.Apis.Auth.OAuth2;
using Melomania.CLI;
using Melomania.Config;
using Melomania.Extractor;
using Melomania.GoogleDrive;
using Melomania.Logging;
using Melomania.Utils;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Melomania
{
    public class Program
    {
        private const string RootCollectionFolderConfigurationKey = "rootCollectionFolder";

        public static async Task Main(string[] args)
        {
            Console.CursorVisible = false;

            args = new[] { "upload", "url", "https://www.youtube.com/watch?v=5N2_eWruhbM", "Ralchev-Kopanica.mp3", "." };

            var configuration = new Configuration();

            // This is indeed a very lame way of parsing the command line parameters, but it allows us to achieve
            // multi-level verbs (e.g. "upload path ...") and async handlers fairly easily. I couldn't find an arguments parser
            // library that could do this without too much code gymnastics
            // TODO: Refactor if the need to support more commands arises.
            if (args.FirstOrDefault() == "setup")
            {
                await Setup(configuration);
            }
            // TODO: This is way too descriptive
            else if (args.FirstOrDefault() == "upload" && args.Length >= 4)
            {
                var subCommand = args[1];

                switch (subCommand)
                {
                    case "path":
                        await UploadFromPath(args, configuration);
                        break;
                    case "url":
                        await UploadFromUrl(args, configuration);
                        break;
                    default:
                        Console.WriteLine($"Command {subCommand} is not supported.");
                        break;
                }
            }
            else
            {
                DisplaySupportedCommands();
            }

            Console.Read();
        }

        private static async Task UploadFromUrl(string[] args, Configuration configuration)
        {
            // TODO: youtube-dl should be embedded or stored inside the config folder
            var trackExtractor = new YoutubeDlTrackExtractor(Directory.GetCurrentDirectory(), /*TODO: Pls*/ Path.Combine(Configuration.RootConfigurationFolder, "_tempFiles"));
            var collection = await GetDriveMusicCollection(configuration);
            var logger = new ConsoleLogger();

            var commandHandler = new UploadFromUrlCommandHandler(trackExtractor, collection, logger);

            var url = args[2];
            var fileName = args[3];
            var destination = args[4];

            var arguments = new UploadFromUrlArguments
            {
                Url = url,
                DestinationInCollection = destination,
                FileName = fileName
            };

            var result = await commandHandler.ExecuteAsync(arguments);

            result.Match(
                some: entry =>
                {
                    Console.WriteLine();
                    Console.WriteLine($"Successfully uploaded '{entry.FileName}'!");
                },
                none: error => Console.WriteLine(error));
        }

        private static void DisplaySupportedCommands()
        {
            Console.WriteLine("Supported commands:");
            Console.WriteLine("'setup'");
            Console.WriteLine("'upload path {folder path} {path inside collection ('.' for root)}'");
            Console.WriteLine("'upload url {url} {file name} {path inside collection ('.' for root)}");
        }

        private static async Task Setup(Configuration configuration)
        {
            // This is implictly going to handle authenticating to Google as their API requires credentials to make a service
            var googleDriveService = await GetGoogleDriveService();

            bool folderExists = false;

            do
            {
                Console.Write("Enter your root music folder (e.g. 'Music'): ");
                var rootCollectionFolder = Console.ReadLine();
                Console.WriteLine();
                var folderIdResult = await googleDriveService.GetFolderIdFromPathAsync(rootCollectionFolder);

                folderIdResult.Match(
                    some: folderId =>
                    {
                        configuration.SetValue(RootCollectionFolderConfigurationKey, folderId);
                        folderExists = true;
                        Console.WriteLine($"Successfully set '{rootCollectionFolder}' (id: {folderId}) as your root collection folder!");
                    },
                    none: error =>
                    {
                        Console.WriteLine(error);
                        Console.WriteLine();
                    });

            } while (!folderExists);
        }

        private static async Task UploadFromPath(string[] args, Configuration configuration)
        {
            var filePath = args[2];
            var destinationFolder = args[3];

            var driveMusicCollection = await GetDriveMusicCollection(configuration);

            var logger = new ConsoleLogger();
            var command = new UploadFromPathCommandHandler(driveMusicCollection, logger);

            SubscribeForProgressChangedEvents(driveMusicCollection);

            var result = await command.ExecuteAsync(new UploadFromPathArguments
            {
                FilePath = filePath,
                FileName = Path.GetFileName(filePath),
                DestinationInCollection = destinationFolder
            });

            result.Match(
                some: entry =>
                {
                    Console.WriteLine();
                    Console.WriteLine($"Successfully uploaded '{entry.FileName}'!");
                },
                none: error => Console.WriteLine(error));
        }

        private static void SubscribeForProgressChangedEvents(GoogleDriveMusicCollection driveMusicCollection)
        {
            driveMusicCollection.OnUploadStarting += entry =>
            {
                Console.WriteLine($"Uploading '{entry.FileName}' to '{entry.DestinationPath}':");
            };

            driveMusicCollection.OnUploadProgressChanged += progress =>
            {
                ClearCurrentLine();
                Console.Write(ProgressBarFromPercentage(progress.Percentage));
            };
        }

        public static void ClearCurrentLine()
        {
            Console.Write("\r" + new string(' ', Console.WindowWidth) + "\r");
            Console.SetCursorPosition(Console.CursorLeft, Console.CursorTop - 1);
        }

        public static string ProgressBarFromPercentage(double percentage)
        {
            var numberOfBars = percentage.RoundToNearestTen() / 10;
            var barFilling = new StringBuilder().Insert(0, "---", numberOfBars).ToString();

            return $"[{string.Format("{0,-30}", barFilling)}]";
        }

        private static async Task<GoogleDriveMusicCollection> GetDriveMusicCollection(Configuration configuration)
        {
            var driveService = await GetGoogleDriveService();

            // TODO: Handle invalid configuration adequately (prompt the user to execute the setup command)
            var collection = new GoogleDriveMusicCollection(driveService, configuration.GetValue(RootCollectionFolderConfigurationKey).ValueOr("Music"));

            return collection;
        }

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
