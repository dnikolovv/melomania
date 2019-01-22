namespace Melomania.Logging
{
    public interface ILogger
    {
        void Write(string message);

        void WriteLine(string message);
    }
}
