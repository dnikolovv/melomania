using Melomania.Music;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Melomania.GoogleDrive
{
    public class GoogleDriveMusicCollection : IMusicCollection
    {
        private const string DriveFolderMimeType = "application/vnd.google-apps.folder";

        public GoogleDriveMusicCollection(GoogleDriveService googleDriveService, string baseCollectionFolder)
        {
            _googleDriveService = googleDriveService;
            _baseCollectionFolder = baseCollectionFolder;
        }

        private readonly GoogleDriveService _googleDriveService;
        private readonly string _baseCollectionFolder;

        public async Task<IEnumerable<MusicCollectionEntry>> GetTracksAsync(int pageSize = 100, string path = null)
        {
            var collectionItems = await _googleDriveService
                .GetFilesAsync(pageSize: pageSize, path: _baseCollectionFolder);

            return collectionItems
                .Select(i => new MusicCollectionEntry
                {
                    Name = i.Name,

                    Type = i.MimeType == DriveFolderMimeType ?
                        MusicCollectionEntryType.Folder :
                        MusicCollectionEntryType.Track
                });
        }

        public Task<UploadTrackResult> UploadTrack(Track track, string folder)
        {
            throw new System.NotImplementedException();
        }
    }
}
