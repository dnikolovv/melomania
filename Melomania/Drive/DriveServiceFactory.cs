﻿using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Melomania.Drive
{
    public class DriveServiceFactory
    {
        public async Task<GoogleDriveService> GetDriveService(string apiSecretsPath)
        {
            if (!File.Exists(apiSecretsPath))
            {
                throw new FileNotFoundException("Please provide a valid path to your credentials.json file (see https://developers.google.com/drive/api/v3/quickstart/dotnet for help).");
            }

            using (var stream = new FileStream(apiSecretsPath, FileMode.Open, FileAccess.Read))
            {
                var tokenSavePath = "~/.melomania/token.json";

                var credentials = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    new[] { DriveService.Scope.Drive },
                    "user",
                    CancellationToken.None,
                    new FileDataStore(tokenSavePath, true));

                return new GoogleDriveService(new DriveService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credentials,
                    ApplicationName = "Melomania"
                }));
            }
        }
    }
}
