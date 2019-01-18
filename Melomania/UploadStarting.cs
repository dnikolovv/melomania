using System;

namespace Melomania
{
    public class UploadStarting
    {
        public string FileName { get; set; }

        public string Path { get; set; }

        public long FileSizeInBytes { get; set; }

        public double FileSizeInMegaBytes => FileSizeInBytes / Math.Pow(1024, 2);
    }
}
