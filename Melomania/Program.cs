using Melomania.GoogleDrive;
using Melomania.Music;
using System;
using System.IO;
using System.Linq;

namespace Melomania
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var driveService = new GoogleDriveServiceFactory()
                .GetDriveService("credentials.json").Result;

            var collection = new GoogleDriveMusicCollection(driveService, "/Music");

            //TestTrackListing(collection);
            using (var trackStream = File.OpenRead("Improvisation 5.mp3"))
            {
                var fileToUpload = new Track { Contents = trackStream, Name = "Improvisation 5TEST3.mp3" };

                Console.WriteLine("Uploading file:");

                driveService.OnUploadStarting += _ =>
                {
                    Console.WriteLine();
                    Console.Write("Progress: [");
                };

                driveService.OnUploadProgressChanged += progress =>
                {
                    Console.Write(new string('-', (int)Math.Round(progress.Percentage / 50)));
                };

                driveService.OnUploadSuccessfull += _ =>
                {
                    Console.Write("]");
                };


                var result = collection.UploadTrack(fileToUpload, "Disk 1 Stamba").Result;
            }


            Console.Read();
        }

        private static void TestTrackListing(GoogleDriveMusicCollection collection)
        {
            var tracks = collection.GetTracksAsync(relativePath: "Disk 1 Stamba").Result;

            var groupedByFolders = tracks.GroupBy(x => x.Type);

            foreach (var group in groupedByFolders)
            {
                Console.WriteLine($"{group.Key}s:");
                Console.WriteLine();

                foreach (var groupEntry in group)
                {
                    Console.WriteLine($"Name: {groupEntry.Name}");
                }

                Console.WriteLine("------------");

            }
        }
    }
}
