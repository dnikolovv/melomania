namespace Melomania.Cloud.Results
{
    public class UploadProgress
    {
        public string FileName { get; set; }

        public long BytesSent { get; set; }

        public long TotalBytesToSend { get; set; }

        public double Percentage => (BytesSent / (double)TotalBytesToSend) * 100;
    }
}
