using System;

namespace Melomania.Cloud.Results
{
    public class UploadFailureResult
    {
        public string FileName { get; set; }

        public string Path { get; set; }

        public Exception Exception { get; set; }
    }
}
