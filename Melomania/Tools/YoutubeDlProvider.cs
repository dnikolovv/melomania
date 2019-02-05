using Optional;
using Optional.Async;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace Melomania.Tools
{
    public class YoutubeDlProvider : IToolsProvider
    {
        public event Action<ToolDownloadInfo> OnToolDownloadCompleted;

        public event Action<ToolDownloadInfo> OnToolDownloadProgressChanged;

        public event Action<ToolDownloadInfo> OnToolDownloadStarting;

        public event Action<ToolDownloadInfo> OnToolIgnored;

        // TODO: Remove hardcoded urls
        public Task<Option<Unit, Error>> DownloadTools(string destination, bool ignoreIfExisting) =>
            Download("youtube-dl.exe", new Uri("https://drive.google.com/uc?export=download&id=1Hs-0-nUwzh59lOakKm1ZzwlrnWKgi1dN"), destination, ignoreIfExisting).FlatMapAsync(_ =>
            Download("ffmpeg.exe", new Uri("https://drive.google.com/uc?export=download&id=1Aaak40dwUhE65tUmgBQi0eRYC5wt-vCc"), destination, ignoreIfExisting)).FlatMapAsync(__ =>
            Download("ffprobe.exe", new Uri("https://drive.google.com/uc?export=download&id=11dSVl6qH23ZklBiQKZit-SkNgZBnO-dT"), destination, ignoreIfExisting));

        private async Task<Option<Unit, Error>> Download(string fileName, Uri url, string destinationFolder, bool ignoreIfExisting)
        {
            using (var webClient = new WebClient())
            {
                try
                {
                    var fullPath = Path.Combine(destinationFolder, fileName);

                    if (!(ignoreIfExisting && File.Exists(fullPath)))
                    {
                        Directory.CreateDirectory(destinationFolder);

                        webClient.DownloadProgressChanged += (sender, args) => OnToolDownloadProgressChanged?.Invoke(new ToolDownloadInfo
                        {
                            Name = fileName,
                            Progress = args.ProgressPercentage
                        });

                        OnToolDownloadStarting?.Invoke(new ToolDownloadInfo { Name = fileName });
                        await webClient.DownloadFileTaskAsync(url, Path.Combine(destinationFolder, fileName));
                        OnToolDownloadCompleted?.Invoke(new ToolDownloadInfo { Name = fileName });
                    }
                    else
                    {
                        OnToolIgnored?.Invoke(new ToolDownloadInfo { Name = fileName });
                    }
                    
                    return Unit.Value.Some<Unit, Error>();
                }
                catch (ArgumentNullException e)
                {
                    return Option.None<Unit, Error>(e.Message);
                }
                catch (WebException e)
                {
                    return Option.None<Unit, Error>(e.Message);
                }
                catch (InvalidOperationException e)
                {
                    return Option.None<Unit, Error>(e.Message);
                }
            }
        }
    }
}