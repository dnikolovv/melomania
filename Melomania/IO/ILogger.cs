namespace Melomania.IO
{
    public interface ILogger
    {
        void Write(string message);

        void WriteLine(string message = null);
    }
}