using Optional;
using System;
using System.Threading.Tasks;

namespace Melomania.Tools
{
    public interface IToolsProvider
    {
        event Action<ToolDownloadInfo> OnToolDownloadStarting;
        event Action<ToolDownloadInfo> OnToolDownloadProgressChanged;
        event Action<ToolDownloadInfo> OnToolDownloadCompleted;
        event Action<ToolDownloadInfo> OnToolIgnored;

        Task<Option<DownloadToolsResult, Error>> DownloadTools(string destination, bool ignoreIfExisting);
    }
}
