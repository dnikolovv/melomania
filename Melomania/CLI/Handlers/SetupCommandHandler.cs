﻿using Melomania.CLI.Arguments;
using Melomania.CLI.Results;
using Melomania.Cloud.GoogleDrive;
using Melomania.Config;
using Optional;
using System.Threading.Tasks;

namespace Melomania.CLI.Handlers
{
    public class SetupCommandHandler : IAsyncCommandHandler<SetupArguments, SetupSuccessfulResult>
    {
        private readonly Configuration _configuration;

        private readonly GoogleDriveService _googleDriveService;

        public SetupCommandHandler(Configuration configuration, GoogleDriveService googleDriveService)
        {
            _googleDriveService = googleDriveService;
            _configuration = configuration;
        }

        public async Task<Option<SetupSuccessfulResult, Error>> ExecuteAsync(SetupArguments arguments)
        {
            var folderIdResult = await _googleDriveService
                .GetFolderIdFromPathAsync(arguments.RootCollectionFolder);

            return folderIdResult
                .Map(folderId =>
                {
                    _configuration.SetRootCollectionFolder(arguments.RootCollectionFolder);
                    return new SetupSuccessfulResult();
                });
        }
    }
}