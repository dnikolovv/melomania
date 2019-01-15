using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Melomania.Drive
{
    public class GoogleDriveMusicCollection : IMusicCollection
    {
        private const string DriveFolderMimeType = "application/vnd.google-apps.folder";

        public GoogleDriveMusicCollection(GoogleDriveService googleDriveService)
        {
            _googleDriveService = googleDriveService;
        }

        private readonly GoogleDriveService _googleDriveService;

        public async Task<IEnumerable<MusicCollectionEntry>> GetTracksAsync(int pageSize = 100)
        {
            var collectionItems = await _googleDriveService
                .GetFilesAsync(pageSize: pageSize, parentFolder: "Music");

            return collectionItems
                .Select(i => new MusicCollectionEntry
                {
                    Name = i.Name,

                    Type = i.MimeType == DriveFolderMimeType ?
                        MusicCollectionEntryType.Folder :
                        MusicCollectionEntryType.Track
                });
        }
    }
}
