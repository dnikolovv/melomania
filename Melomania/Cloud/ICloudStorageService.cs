using Melomania.Cloud.Results;
using Optional;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Melomania.Cloud
{
    public interface ICloudStorageService
    {
        event Action<UploadFailureResult> OnUploadFailure;
        event Action<UploadProgress> OnUploadProgressChanged;
        event Action<UploadStarting> OnUploadStarting;
        event Action<UploadSuccessResult> OnUploadSuccessfull;

        Task<Option<List<CloudFile>, Error>> GetFilesAsync(string path, int pageSize = 100);
        Task<Option<string, Error>> GetFolderIdFromPathAsync(string folderPath);
        Task<Option<CloudFile, Error>> UploadFile(Stream fileContents, string fileName, string path);
    }
}