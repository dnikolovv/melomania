using Google.Apis.Auth.OAuth2;
using Melomania.CLI;
using Melomania.Config;
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
        public static async Task Main(string[] args)
        {
            Console.CursorVisible = false;

            args = new[] { "upload", "path", "Improvisation 5.mp3", "asdfasdf/asdfasdf" };

            var configuration = new Configuration();

            // This is indeed a very lame way of parsing the command line parameters, but it allows us to achieve
            // multi-level verbs (e.g. "upload path ...") and async handlers fairly easily. I couldn't find an arguments parser
            // library that could do this without too much code gymnastics
            // TODO: Refactor if the need to support more commands arises.
            if (args.FirstOrDefault() == "setup")
            {
                throw new NotImplementedException();
            }
            // TODO: This is way too descriptive
            else if (args.FirstOrDefault() == "upload")
            {
                if (args.Length < 4)
                {
                    Console.WriteLine("Supported commands:");
                    Console.WriteLine("'upload path {folder path} {path inside collection ('.' for root)}'");
                    Console.WriteLine("'upload url {url} {path inside collection ('.' for root)}");
                }
                else
                {
                    var subCommand = args[1];

                    switch (subCommand)
                    {
                        case "path":
                            await UploadFromPath(args, configuration);
                            break;
                        case "url":
                            throw new NotImplementedException();
                        default:
                            Console.WriteLine($"Command {subCommand} is not supported.");
                            break;
                    }
                }
            }
            else
            {
                Console.WriteLine("Invalid arguments.");
            }
        }

        private static async Task UploadFromPath(string[] args, Configuration configuration)
        {
            var filePath = args[2];
            var destinationFolder = args[3];

            var driveMusicCollection = await GetDriveMusicCollection(configuration);

            var logger = new ConsoleLogger();
            var command = new UploadFromPathCommand(driveMusicCollection, logger);

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
                Console.WriteLine($"Uploading {entry.FileName}:");
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
            using (var stream = new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
            {
                var clientSecrets = GoogleClientSecrets.Load(stream).Secrets;

                var driveService = await new GoogleDriveServiceFactory()
                    .GetDriveService(clientSecrets);

                                                                              // TODO: Handle invalid configuration adequately
                var collection = new GoogleDriveMusicCollection(driveService, configuration.GetValue("rootCollectionFolder").ValueOr("Music"));

                return collection;
            }
        }
    }
}
