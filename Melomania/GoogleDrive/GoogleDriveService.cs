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
    public class GoogleDriveService
    {
        public GoogleDriveService(DriveService driveService)
        {
            _driveService = driveService;
        }

        private readonly DriveService _driveService;

        /// <summary>
        /// Retrieves a list of files in a given path. Implicitly filters for unexisting files.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="pageSize">The maximum page size.</param>
        /// <returns>A list of files.</returns>
        public async Task<IList<File>> GetFilesAsync(string path, int pageSize = 100)
        {
            var listRequest = _driveService.Files.List();

            listRequest.PageSize = pageSize;
            listRequest.Fields = "nextPageToken, files(fileExtension,id,name,size,webContentLink,webViewLink,mimeType,parents,properties,videoMediaMetadata,appProperties)";

            if (!string.IsNullOrEmpty(path))
            {
                var folderId = await GetFolderIdFromPathAsync(path);
                
                // TODO: Decide whether to create a new folder if the path doesn't exist or return an error.
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


        /// <summary>
        /// Gets the deepest folder in a path's id. (e.g. \Root\Subfolder1\Subfolder2 will return "Subfolder2"'s id).
        /// </summary>
        /// <param name="folderPath">The folder path.</param>
        /// <returns>The deepest folder in the path's id or an error.</returns>
        private Task<Option<string, Error>> GetFolderIdFromPathAsync(string folderPath) =>
            SplitPath(folderPath).FlatMapAsync(folderHierarchy =>
            ExtractDeepestFolderId(folderHierarchy));

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
                                   var parentIds = alreadyFetchedFolderIds.Take(i);

                                   var currentFolderIdResult = await GetFolderIdAsync(currentFolderName, parentIds);

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
        /// Retrieves a folder id by name.
        /// To make sure that multiple folders in the same drive don't mess things up,
        /// you are required to provide a list of parent ids.
        /// If none are provided, the function is implicitly going to check the root folder.
        /// </summary>
        /// <param name="folderName">The folder name.</param>
        /// <param name="parentIds">A list of parent ids.</param>
        /// <returns>The folder id or an error.</returns>
        private async Task<Option<string, Error>> GetFolderIdAsync(string folderName, IEnumerable<string> parentIds = null)
        {
            try
            {
                var folderInfoRequest = _driveService.Files.List();

                var parentsQuery = GenerateParentsQuery(parentIds);

                folderInfoRequest.Fields = "files(id,name,parents)";
                folderInfoRequest.Q = $"mimeType = 'application/vnd.google-apps.folder' and" +
                                      $"name = '{folderName}' and" +
                                      $"{parentsQuery}";

                var results = (await folderInfoRequest.ExecuteAsync()).Files;

                return results
                    .SomeWhen<IList<File>, Error>(
                        rs => rs?.Count == 1,
                        $"Multiple folders with the name {folderName} with parents {parentsQuery} were found when one was expected.")
                    .Map(rs => rs.Single().Id);
            }
            // TODO: Research the exact exception that Google throws when the file is not found.
            catch (Exception e)
            {
                // TODO: A more descriptive error?
                return Option.None<string, Error>($"Could not find folder {folderName}.");
            }
        }

        /// <summary>
        /// Generates an "in parents" query. If no parent ids were supplied, the function is implicitly going to generate a "'root' in parents" query.
        /// </summary>
        /// <param name="parentIds">The parent ids.</param>
        /// <returns>An "in parents" query. (e.g. "'1234123' in parents")</returns>
        private string GenerateParentsQuery(IEnumerable<string> parentIds)
        {
            if (parentIds?.Any() == true)
            {
                // Wrap each parent in single quotes, as that is how the API expects it.
                var parentsStrings = parentIds.Select(p => $"'{p.Trim()}'");

                return $"{string.Join(", ", parentsStrings)} in parents";
            }

            return "'root' in parents";
        }

        /// <summary>
        /// Converts a path into an array. Ex. "/Root/Subfolder1/Subfolder2" becomes ["Root", "Subfolder1", "Subfolder2"] (in order).
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>The split path or nothing.</returns>
        private Option<string[], Error> SplitPath(string path) =>
            path.SomeNotNull<string, Error>($"The path must not be null.")
                .Map(p => p.Split(new char[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries));
    }
}
