using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Optional;
using Optional.Async;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Melomania.GoogleDrive
{
    // TODO: The error handling and propagation is a total mess
    public class GoogleDriveService
    {
        public GoogleDriveService(DriveService driveService)
        {
            _driveService = driveService;
        }

        private readonly DriveService _driveService;

        public async Task<IList<File>> GetFilesAsync(string path, int pageSize = 100)
        {
            var listRequest = _driveService.Files.List();

            listRequest.PageSize = pageSize;
            listRequest.Fields = "nextPageToken, files(fileExtension,id,name,size,webContentLink,webViewLink,mimeType,parents,properties,videoMediaMetadata,appProperties)";

            if (!string.IsNullOrEmpty(path))
            {
                var folderId = await GetFolderIdFromPathAsync(path);

                // TODO: Shitty code, chain those
                folderId.MatchSome(id =>
                    listRequest.Q = $"'{id}' in parents");
            }

            var files = (await listRequest.ExecuteAsync()).Files;

            return files;
        }

        //public async Task<Option<File>> UploadFile(System.IO.FileStream fileContents, string fileName, string parentFolder, string fileType)
        //{
        //    var parentFolderId = await GetFolderIdFromPathAsync(parentFolder);

        //    var fileMetadata = new File()
        //    {
        //        Name = fileName,
        //        Parents = new[] { parentFolderId }
        //    };

        //    var uploadRequest = _driveService.Files.Create(fileMetadata, fileContents, fileType);

        //    // Return the id and name fields when finished uploading
        //    uploadRequest.Fields = "id,name";

        //    await uploadRequest.UploadAsync();

        //    var uploadedFile = uploadRequest.ResponseBody;

        //    return uploadedFile;
        //}

        // split the path (e.g.) "/Music/Album1/Songs becomes ["Music", "Album1", "Songs"] (in order)
        // for each folder in path, look for its id (the first folder's parents collection must be empty)
        // return the last folder in the path's id
        private Task<Option<string>> GetFolderIdFromPathAsync(string folderPath) =>
            SplitPath(folderPath).FlatMapAsync(folderHierarchy =>
            ExtractDeepestFolderId(folderHierarchy));

        private Task<Option<string>> ExtractDeepestFolderId(string[] folderHierarchy) =>
            folderHierarchy.SomeWhen(x => x.Length > 0)
                           .MapAsync(async folders =>
                           {
                               // As we must make sure that the first folder is in the root space
                               // we query for it separately

                               var alreadyFetchedFolderIds = new List<string>(folderHierarchy.Length);

                               for (int i = 0; i < folders.Length; i++)
                               {
                                   var currentFolderName = folders[i];
                                   var parentIds = alreadyFetchedFolderIds.Take(i);

                                   var currentFolderId = await GetFolderIdAsync(currentFolderName, parentIds);

                                   // TODO: It's assumed that if the id couldn't be fetched an exception would've been thrown
                                   // fix the design to use Either everywhere
                                   currentFolderId.MatchSome(id => alreadyFetchedFolderIds.Add(id));                                   
                               };

                               return alreadyFetchedFolderIds.Last();
                           });

        private async Task<Option<string>> GetFolderIdAsync(string folderName, IEnumerable<string> parentIds)
        {
            var folderInfoRequest = _driveService.Files.List();

            folderInfoRequest.Fields = "files(id,name,parents)";

            folderInfoRequest.Q = $"mimeType = 'application/vnd.google-apps.folder' and name = '{folderName}'";

            // Wrap each parent in single quotes as that is how the API expects them to be
            var parentsQuery = string.Join(", ", parentIds.Select(p => $"'{p.Trim()}'"));

            // TODO: Make pretty
            if (!string.IsNullOrEmpty(parentsQuery))
            {
                folderInfoRequest.Q += $" and ({parentsQuery} in parents)";
            }
            else
            {
                folderInfoRequest.Q += " and ('root' in parents)";
            }

            var results = (await folderInfoRequest.ExecuteAsync()).Files;

            // TODO: Use Either
            if (results.Count > 1)
            {
                throw new InvalidOperationException($"Multiple folders with the name {folderName} with parents {parentsQuery} were found. " +
                    $"Please make sure that your music collection resides in one folder with unique name.");
            }

            var folderId = results.Single().Id;

            return folderId.Some();
        }

        private Option<string[]> SplitPath(string path) =>
            path.SomeNotNull()
                .Map(p => p.Split(new char[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries));
    }
}
