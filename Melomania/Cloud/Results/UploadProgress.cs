namespace Melomania.Cloud.Results
{
    public class UploadProgress
    {
        public long BytesSent { get; set; }
        public string FileName { get; set; }
        public double Percentage => (BytesSent / (double)TotalBytesToSend) * 100;
        public long TotalBytesToSend { get; set; }
    }
}