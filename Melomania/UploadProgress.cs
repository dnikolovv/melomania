namespace Melomania
{
    public class UploadProgress
    {
        public long BytesSent { get; set; }

        public long TotalBytesToSend { get; set; }

        public double Percentage => (BytesSent / (double)TotalBytesToSend) * 100;
    }
}
