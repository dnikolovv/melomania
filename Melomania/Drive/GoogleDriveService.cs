using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using System.IO;

namespace Melomania.Drive
{
    public class GoogleDriveService
    {
        private static string[] Scopes = { DriveService.Scope.Drive };
        private static string ApplicationName = "Melomania";

        private readonly DriveService _driveService;

        public UserCredential Authorize(string credentialsPath)
        {
            if (!File.Exists(credentialsPath))
            {
                throw new FileNotFoundException("Please provide a valid path to your credentials.json file (see https://developers.google.com/drive/api/v3/quickstart/dotnet for help).");
            }

            using (var stream = new FileStream(credentialsPath, FileMode.Open, FileAccess.Read))
            {
                // The file token.json stores the user's access and refresh tokens, and is created
                // automatically when the authorization flow completes for the first time.
                string credPath = "token.json";
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
                Console.WriteLine("Credential file saved to: " + credPath);
            }
        }
    }
}
