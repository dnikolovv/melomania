using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Melomania.Drive
{
    public class GoogleDriveService
    {
        public GoogleDriveService(DriveService driveService)
        {
            _driveService = driveService;
        }

        private readonly DriveService _driveService;

        public async Task<IList<File>> GetFilesAsync(int pageSize = 100, string parentFolder = null)
        {
            var listRequest = _driveService.Files.List();

            listRequest.PageSize = pageSize;
            listRequest.Fields = "nextPageToken, files(fileExtension,id,name,size,webContentLink,webViewLink,mimeType,parents)";

            if (!string.IsNullOrEmpty(parentFolder))
            {
                var folderId = await GetFolderIdAsync(parentFolder);
                listRequest.Q = $"'{folderId}' in parents";
            }

            var files = (await listRequest.ExecuteAsync()).Files;

            return files;
        }

        private async Task<string> GetFolderIdAsync(string folderName)
        {
            var folderInfoRequest = _driveService.Files.List();

            folderInfoRequest.Q = $"mimeType = 'application/vnd.google-apps.folder' and name = '{folderName}'";
            folderInfoRequest.Fields = "files(contentHints/thumbnail,fileExtension,iconLink,id,name,size,thumbnailLink,webContentLink,webViewLink,mimeType,parents)";

            var results = (await folderInfoRequest.ExecuteAsync()).Files;

            if (results.Count > 1)
            {
                throw new InvalidOperationException($"Multiple folders with the name {folderName} were found. " +
                    $"Please make sure that your music collection resides in one folder with unique name.");
            }

            return results.Single().Id;
        }
    }
}
