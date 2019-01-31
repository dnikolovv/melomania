using System;

namespace Melomania.Cloud.Results
{
    public class UploadFailureResult
    {
        public Exception Exception { get; set; }
        public string FileName { get; set; }
        public string Path { get; set; }
    }
}