using Melomania.Cloud.GoogleDrive;
using Melomania.Config;
using Optional;

namespace Melomania.Music.GoogleDrive
{
    public class GoogleDriveMusicCollectionFactory : IMusicCollectionFactory
    {
        public GoogleDriveMusicCollectionFactory(GoogleDriveService googleDriveService, Configuration configuration)
        {
            _googleDriveService = googleDriveService;
            _configuration = configuration;
        }

        private readonly GoogleDriveService _googleDriveService;
        private readonly Configuration _configuration;

        public Option<IMusicCollection, Error> GetMusicCollection() =>
            _configuration
                .GetRootCollectionFolder()
                .WithException<Error>("Root collection folder not set. Please run 'melomania setup'.")
                .Map<IMusicCollection>(rootCollectionFolder => new GoogleDriveMusicCollection(_googleDriveService, rootCollectionFolder));
    }
}
