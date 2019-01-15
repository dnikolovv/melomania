using Melomania.Drive;
using System;
using System.Linq;

namespace Melomania
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var driveService = new DriveServiceFactory()
                .GetDriveService("credentials.json").Result;

            var store = new GoogleDriveMusicCollection(driveService);

            var test = store.GetTracksAsync(1000).Result;

            var groupedByFolders = test.GroupBy(x => x.Type);

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
