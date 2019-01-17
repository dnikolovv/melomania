using Melomania.Music;
using Optional;
using Optional.Async;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Melomania.GoogleDrive
{
    public class GoogleDriveMusicCollection : IMusicCollection
    {
        private const string DriveFolderMimeType = "application/vnd.google-apps.folder";

        public GoogleDriveMusicCollection(GoogleDriveService googleDriveService, string baseCollectionFolder)
        {
            _googleDriveService = googleDriveService ?? throw new ArgumentNullException(nameof(googleDriveService));
            _baseCollectionFolder = baseCollectionFolder ?? throw new ArgumentNullException(nameof(baseCollectionFolder));
        }

        private readonly GoogleDriveService _googleDriveService;
        private readonly string _baseCollectionFolder;

        /// <summary>
        /// Retrieves a list of tracks from a music collection.
        /// </summary>
        /// <param name="pageSize">The page size.</param>
        /// <param name="relativePath">A path relative to the collection root path.</param>
        /// <returns>A collection of music entries.</returns>
        public async Task<IEnumerable<MusicCollectionEntry>> GetTracksAsync(int pageSize = 100, string relativePath = null)
        {
            var fullPath = GenerateFullPath(relativePath);

            var collectionItems = await _googleDriveService
                .GetFilesAsync(pageSize: pageSize, path: fullPath);

            return collectionItems
                .Select(i => new MusicCollectionEntry
                {
                    Name = i.Name,

                    Type = i.MimeType == DriveFolderMimeType ?
                        MusicCollectionEntryType.Folder :
                        MusicCollectionEntryType.Track
                });
        }

        public Task<Option<MusicCollectionEntry, Error>> UploadTrack(Track track, string relativePath)
        {
            // TODO: Make pretty
            return _googleDriveService.UploadFile(track.Contents, track.Name, GoogleDriveFileContentType.Audio, GenerateFullPath(relativePath)).MapAsync(async file =>
            new MusicCollectionEntry
            {
                Name = file.Name,
                Type = MusicCollectionEntryType.Track
            });
        }

        private string GenerateFullPath(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath))
            {
                return _baseCollectionFolder;
            }

            return Path.Combine(_baseCollectionFolder, relativePath);
        }
    }
}
