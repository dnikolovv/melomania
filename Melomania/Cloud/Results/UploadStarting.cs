using System;

namespace Melomania.Cloud.Results
{
    public class UploadStarting
    {
        public string FileName { get; set; }

        public string DestinationPath { get; set; }

        public long FileSizeInBytes { get; set; }

        public double FileSizeInMegaBytes => FileSizeInBytes / Math.Pow(1024, 2);
    }
}
