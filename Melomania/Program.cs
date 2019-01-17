using Melomania.GoogleDrive;
using System;
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

            Console.Read();
        }
    }
}
