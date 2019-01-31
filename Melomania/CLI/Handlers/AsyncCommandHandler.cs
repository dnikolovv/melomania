using Optional;
using System.Threading.Tasks;

namespace Melomania.CLI.Handlers
{
    public interface IAsyncCommandHandler<TArguments, TResult>
    {
        Task<Option<TResult, Error>> ExecuteAsync(TArguments arguments);
    }
}