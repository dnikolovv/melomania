using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Melomania.Config;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Melomania.Cloud.GoogleDrive
{
    public class GoogleDriveServiceFactory
    {
        public async Task<GoogleDriveService> GetDriveService(ClientSecrets clientSecrets, Configuration configuration)
        {
            var tokenSavePath = Path.Combine(configuration.RootConfigurationFolder, "google");

            var credentials = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                clientSecrets,
                new[]
                {
                    DriveService.Scope.Drive,
                    DriveService.Scope.DriveAppdata,
                    DriveService.Scope.DriveFile,
                    DriveService.Scope.DriveMetadataReadonly,
                    DriveService.Scope.DriveReadonly,
                    DriveService.Scope.DriveScripts
                },
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