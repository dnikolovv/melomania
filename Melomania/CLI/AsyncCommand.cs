using Melomania.Logging;
using Optional;
using System.Threading.Tasks;

namespace Melomania.CLI
{
    public abstract class AsyncCommand<TArguments, TResult> : IAsyncCommand
    {
        public AsyncCommand(ILogger logger)
        {
            Logger = logger;
        }

        protected ILogger Logger { get; }

        public abstract Task<Option<TResult, Error>> ExecuteAsync(TArguments arguments);
    }
}
