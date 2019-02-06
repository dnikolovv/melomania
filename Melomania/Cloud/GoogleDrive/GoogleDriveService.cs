using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Upload;
using Melomania.Cloud.Results;
using Melomania.Utils;
using Optional;
using Optional.Async;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Melomania.Cloud.GoogleDrive
{
    // TODO: This should be abstracted into a service that can handle all types of cloud storage providers e.g. Dropbox
    public class GoogleDriveService : ICloudStorageService
    {
        private readonly DriveService _driveService;

        public GoogleDriveService(DriveService driveService)
        {
            _driveService = driveService;
        }

        public event Action<UploadFailureResult> OnUploadFailure;

        public event Action<UploadProgress> OnUploadProgressChanged;

        public event Action<UploadStarting> OnUploadStarting;

        public event Action<UploadSuccessResult> OnUploadSuccessfull;

        /// <summary>
        /// Retrieves a list of files in a given path. Implicitly filters for deleted files.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="pageSize">The maximum page size.</param>
        /// <returns>A list of files.</returns>
        public Task<Option<List<CloudFile>, Error>> GetFilesAsync(string path, int pageSize = 100) =>
            path.SomeNotNull((Error)"You must provide a non-null path.")
                .FlatMapAsync(_ => GetFolderIdFromPathAsync(path))
                .MapAsync(async parentFolderId =>
                {
                    var listRequest = _driveService.Files.List();

                    listRequest.PageSize = pageSize;
                    listRequest.Fields = "nextPageToken, files(fileExtension,id,name,size,webContentLink,webViewLink,mimeType,parents,properties,videoMediaMetadata,appProperties,trashed,explicitlyTrashed)";
                    listRequest.Q = $"trashed = false and '{parentFolderId}' in parents";

                    var files = (await listRequest.ExecuteAsync()).Files;

                    return files;
                })
                .MapAsync(async files => files.Select(ToCloudFile).ToList());

        /// <summary>
        /// Gets the deepest folder in a path's id. (e.g. \Root\Subfolder1\Subfolder2 will return "Subfolder2"'s id).
        /// </summary>
        /// <param name="folderPath">The folder path.</param>
        /// <returns>The deepest folder in the path's id or an error.</returns>
        public Task<Option<string, Error>> GetFolderIdFromPathAsync(string folderPath) =>
            SplitPath(folderPath).FlatMapAsync(folderHierarchy =>
            ExtractDeepestFolderId(folderHierarchy)).
            MapExceptionAsync(error => Error.MergeErrors(error, $"Searched path: '{folderPath}'."));

        /// <summary>
        /// Uploads a file to a given path.
        /// </summary>
        /// <param name="fileContents">The file contents.</param>
        /// <param name="fileName">The file name (including extension).</param>
        /// <param name="fileType">The file content type.</param>
        /// <param name="path">The path to upload the file to.</param>
        /// <returns>Either the uploaded file or an error.</returns>
        public Task<Option<CloudFile, Error>> UploadFile(System.IO.Stream fileContents, string fileName, string path) =>
            // TODO: Decide whether to create a new folder if the provided doesn't exist
            GetFolderIdFromPathAsync(path).MapAsync(async parentFolderId =>
            {
                var fileMetadata = new File()
                {
                    Name = fileName,
                    Parents = new[] { parentFolderId }
                };

                var uploadRequest = _driveService
                    .Files
                    .Create(fileMetadata, fileContents, fileMetadata.MimeType);

                // Return the id and name fields when finished uploading
                uploadRequest.Fields = "id,name";
                uploadRequest.ChunkSize = ResumableUpload.MinimumChunkSize;

                uploadRequest.ProgressChanged += progress =>
                {
                    switch (progress.Status)
                    {
                        case UploadStatus.NotStarted:
                            break;

                        case UploadStatus.Starting:
                            OnUploadStarting?.Invoke(new UploadStarting { FileName = fileName, DestinationPath = path, FileSizeInBytes = fileContents.Length });
                            break;

                        case UploadStatus.Uploading:
                            OnUploadProgressChanged?.Invoke(new UploadProgress { FileName = fileName, BytesSent = progress.BytesSent, TotalBytesToSend = fileContents.Length });
                            break;

                        case UploadStatus.Completed:
                            OnUploadSuccessfull?.Invoke(new UploadSuccessResult { FileName = fileName, Path = path });
                            break;

                        case UploadStatus.Failed:
                            OnUploadFailure?.Invoke(new UploadFailureResult { FileName = fileName, Path = path, Exception = progress.Exception });
                            break;

                        default:
                            break;
                    }
                };

                var result = await uploadRequest.UploadAsync();

                var uploadedFile = uploadRequest.ResponseBody;

                return uploadedFile;
            })
            .MapAsync(async f => ToCloudFile(f));

        private static CloudFile ToCloudFile(File f) => new CloudFile
        {
            Name = f.Name,
            MimeType = f.MimeType
        };

        /// <summary>
        /// Gets the deepest folder in a hierarchy's id. (e.g. ["Root", "Subfolder1", "Subfolder2"] will return "Subfolder2"'s id).
        /// </summary>
        /// <param name="folderHierarchy">The folder hierarchy.</param>
        /// <returns>The deepest folder's id.</returns>
        private Task<Option<string, Error>> ExtractDeepestFolderId(string[] folderHierarchy) =>
            folderHierarchy.SomeWhen<string[], Error>(x => x?.Length > 0, "The folder hierarchy must be at least one level deep.")
                           .FlatMapAsync(async folders =>
                           {
                               var alreadyFetchedFolderIds = new List<string>(folderHierarchy.Length);

                               for (int i = 0; i < folders.Length; i++)
                               {
                                   var currentFolderName = folders[i];
                                   var parentId = alreadyFetchedFolderIds.Take(i).LastOrDefault();

                                   var currentFolderIdResult = await GetFolderIdAsync(currentFolderName, parentId);

                                   // TODO: This is one ugly function :)
                                   if (!currentFolderIdResult.HasValue)
                                   {
                                       return currentFolderIdResult;
                                   }

                                   currentFolderIdResult.MatchSome(id => alreadyFetchedFolderIds.Add(id));
                               };

                               return alreadyFetchedFolderIds
                                   .Last()
                                   .Some<string, Error>();
                           });

        /// <summary>
        /// Generates an "in parents" query. If no parent ids were supplied, the function is implicitly going to generate a "'root' in parents" query.
        /// </summary>
        /// <param name="parentIds">The parent ids.</param>
        /// <returns>An "in parents" query. (e.g. "'1234123' in parents")</returns>
        private string GenerateParentsQuery(string parentId)
        {
            if (string.IsNullOrEmpty(parentId))
            {
                return "'root' in parents";
            }

            // Wrap the parent in single quotes, as that is how the API expects it.
            var parentString = $"'{parentId.Trim()}'";

            return $"{parentString} in parents";
        }

        /// <summary>
        /// Retrieves a folder id by name.
        /// To make sure that multiple folders in the same drive don't mess things up,
        /// you are required to provide a parent id.
        /// If none are provided, the function is implicitly going to check the root folder.
        /// </summary>
        /// <param name="folderName">The folder name.</param>
        /// <param name="parentId">The parent id.</param>
        /// <returns>The folder id or an error.</returns>
        private async Task<Option<string, Error>> GetFolderIdAsync(string folderName, string parentId)
        {
            var folderInfoRequest = _driveService.Files.List();

            var parentsQuery = GenerateParentsQuery(parentId);

            folderInfoRequest.Fields = "files(id,name,parents)";
            folderInfoRequest.Q = $"mimeType = 'application/vnd.google-apps.folder' and " +
                                  $"trashed = false and " +
                                  $"name = '{folderName}' and " +
                                  $"{parentsQuery}";

            var results = (await folderInfoRequest.ExecuteAsync()).Files;

            return results
                .Some<IList<File>, Error>()
                .Filter(rs => rs?.Count > 0, $"No folder with the name '{folderName}' was found.")
                .Filter(rs => rs?.Count == 1, $"Multiple folders with the name '{folderName}' were found when one was expected.")
                .Map(rs => rs.Single().Id);
        }

        /// <summary>
        /// Converts a path into an array. Ex. "/Root/Subfolder1/Subfolder2" becomes ["Root", "Subfolder1", "Subfolder2"] (in order).
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>The split path or nothing.</returns>
        private Option<string[], Error> SplitPath(string path) =>
            path.SomeNotNull<string, Error>($"The path must not be null.")
                .Map(p => p.Split(new char[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries))
                // Takes care of multiple slashes between paths (e.g. ///Music///Disco
                .Map(ps => ps.Select(p => p.Trim(new[] { '/', '\\' })).ToArray());
    }
}