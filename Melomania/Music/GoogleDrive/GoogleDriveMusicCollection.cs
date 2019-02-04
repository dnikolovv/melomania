using Melomania.Cloud.GoogleDrive;
using Melomania.Cloud.Results;
using Optional;
using Optional.Async;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Melomania.Music.GoogleDrive
{
    public class GoogleDriveMusicCollection : IMusicCollection
    {
        private const string DriveFolderMimeType = "application/vnd.google-apps.folder";

        private readonly string _baseCollectionFolder;

        private readonly GoogleDriveService _googleDriveService;

        public GoogleDriveMusicCollection(GoogleDriveService googleDriveService, string baseCollectionFolder)
        {
            _googleDriveService = googleDriveService ?? throw new ArgumentNullException(nameof(googleDriveService));
            _baseCollectionFolder = baseCollectionFolder ?? throw new ArgumentNullException(nameof(baseCollectionFolder));
        }

        public event Action<UploadFailureResult> OnUploadFailure
        {
            add { _googleDriveService.OnUploadFailure += value; }
            remove { _googleDriveService.OnUploadFailure -= value; }
        }

        public event Action<UploadProgress> OnUploadProgressChanged
        {
            add { _googleDriveService.OnUploadProgressChanged += value; }
            remove { _googleDriveService.OnUploadProgressChanged -= value; }
        }

        public event Action<UploadStarting> OnUploadStarting
        {
            add { _googleDriveService.OnUploadStarting += value; }
            remove { _googleDriveService.OnUploadStarting -= value; }
        }

        public event Action<UploadSuccessResult> OnUploadSuccessfull
        {
            add { _googleDriveService.OnUploadSuccessfull += value; }
            remove { _googleDriveService.OnUploadSuccessfull -= value; }
        }

        /// <summary>
        /// Retrieves a list of tracks from a music collection.
        /// </summary>
        /// <param name="pageSize">The page size.</param>
        /// <param name="relativePath">A path relative to the collection root path.</param>
        /// <returns>A collection of music entries.</returns>
        public async Task<Option<IEnumerable<MusicCollectionEntry>, Error>> GetEntriesAsync(int pageSize = 100, string relativePath = null)
        {
            var fullPath = GenerateFullPath(relativePath);

            var collectionItems = await _googleDriveService
                .GetFilesAsync(pageSize: pageSize, path: fullPath);

            return collectionItems
                .Map(entries => entries
                    .Select(i => new MusicCollectionEntry
                    {
                        Name = i.Name,

                        Type = i.MimeType == DriveFolderMimeType ?
                            MusicCollectionEntryType.Folder :
                            MusicCollectionEntryType.Track
                    }))
                .MapException(error => error += $"Note that you must provide a path relative to your base collection folder. (currently '{_baseCollectionFolder}')");
        }

        /// <summary>
        /// Uploads a track to your music collection.
        /// </summary>
        /// <param name="track">The track.</param>
        /// <param name="relativePath">The path relative to your music collection root.</param>
        /// <returns>The new entry or an error.</returns>
        public Task<Option<MusicCollectionEntry, Error>> UploadTrackAsync(Track track, string relativePath) =>
            _googleDriveService.UploadFile(
                track.Contents,
                track.Name,
                GoogleDriveFileContentType.Audio,
                GenerateFullPath(relativePath))
            .MapAsync(async file => new MusicCollectionEntry
            {
                Name = file.Name,
                Type = MusicCollectionEntryType.Track
            });

        private string GenerateFullPath(string relativePath)
        {
            // We interpret '.' or empty paths as the base folder
            if (string.IsNullOrEmpty(relativePath))
            {
                return _baseCollectionFolder;
            }

            return relativePath == "." ?
                _baseCollectionFolder :
                Path.Combine(_baseCollectionFolder, relativePath);
        }
    }
}