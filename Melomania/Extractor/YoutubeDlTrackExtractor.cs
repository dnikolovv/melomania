using Melomania.Music;
using NYoutubeDL;
using NYoutubeDL.Models;
using Optional;
using Optional.Async;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Melomania.Extractor
{
    /// <summary>
    /// Uses the youtube-dl tool with ffmpeg and ffprobe to extract .mp3 tracks from urls.
    /// </summary>
    public class YoutubeDlTrackExtractor : ITrackExtractor
    {
        private readonly string _tempFolder;

        private readonly string _toolsFolder;

        public YoutubeDlTrackExtractor(string toolsFolder, string tempFolder)
        {
            _toolsFolder = toolsFolder;
            _tempFolder = tempFolder;
        }

        public event Action<TrackExtractionInfo> OnExtractionFinished;

        public event Action<TrackExtractionInfo> OnExtractionProgressChanged;

        public event Action<TrackExtractionInfo> OnExtractionStarting;

        public Task<Option<Track, Error>> ExtractTrackFromUrl(string url) =>
            CheckForFfmpeg(_toolsFolder).FlatMapAsync(ffmpegPath =>
            GetYoutubeDl(_toolsFolder, ffmpegPath, _tempFolder).FlatMapAsync(youtubeDl =>
            GetDownloadInfo(youtubeDl, url).FlatMapAsync(downloadInfo =>
            DownloadTrackAsync(youtubeDl, downloadInfo, url, _tempFolder))));

        private Option<string, Error> CheckForFfmpeg(string toolsPath) =>
            toolsPath
                .SomeWhen<string, Error>(f => !string.IsNullOrEmpty(f), "Invalid tools folder.")
                .Filter(_ =>
                {
                    var directoryFiles = Directory
                        .GetFiles(toolsPath)
                        .Select(path => Path.GetFileName(path));

                    var ffmpegExists = directoryFiles.Any(f => f == "ffmpeg.exe");
                    var ffProbeExists = directoryFiles.Any(f => f == "ffprobe.exe");

                    return ffmpegExists && ffProbeExists;
                }, $"ffmpeg or ffprobe couldn't be found inside {toolsPath}.");

        private Task<Option<Track, Error>> DownloadTrackAsync(YoutubeDL youtubeDl, DownloadInfo downloadInfo, string url, string tempFolder) =>
            url.SomeWhen<string, Error>(x => !string.IsNullOrEmpty(x), $"Invalid url")
               .MapAsync(async _ =>
               {
                   // TODO: Shorten
                   var fileName = $"{RemoveSequentialWhitespaces(downloadInfo.Title)}.mp3";
                   var outputPath = Path.Combine(tempFolder, fileName);

                   youtubeDl.Options.FilesystemOptions.Output = outputPath;
                   youtubeDl.VideoUrl = url;
                   youtubeDl.Info.PropertyChanged += (sender, args) =>
                   {
                       if (sender is DownloadInfo info && args.PropertyName == nameof(DownloadInfo.VideoProgress))
                       {
                           OnExtractionProgressChanged?.Invoke(new TrackExtractionInfo
                           {
                               Title = fileName,
                               Progress = info.VideoProgress
                           });
                       }
                   };

                   var trackInfo = new TrackExtractionInfo
                   {
                       Title = fileName
                   };

                   OnExtractionStarting?.Invoke(trackInfo);

                   await youtubeDl.PrepareDownloadAsync();
                   await youtubeDl.DownloadAsync();

                   OnExtractionFinished?.Invoke(trackInfo);

                   var fileStream = File.OpenRead(outputPath);
                   var memoryStream = new MemoryStream(new byte[fileStream.Length]);
                   await fileStream.CopyToAsync(memoryStream);
                   fileStream.Close();
                   File.Delete(outputPath);

                   return new Track
                   {
                       Contents = memoryStream,
                       Name = fileName
                   };
               });

        // TODO: Handle multi download info (there is a MultiDownloadInfo class inheriting DownloadInfo)
        private Task<Option<DownloadInfo, Error>> GetDownloadInfo(YoutubeDL youtubeDl, string url) =>
            url.SomeWhen<string, Error>(x => !string.IsNullOrEmpty(x), $"Invalid url.")
               .MapAsync(_ => youtubeDl.GetDownloadInfoAsync(url));

        private Option<YoutubeDL, Error> GetYoutubeDl(string toolsPath, string ffmpegPath, string outputPath) =>
            toolsPath
                .SomeWhen<string, Error>(f => !string.IsNullOrEmpty(f), "Invalid tools folder.")
                .Map(f => Path.Combine(toolsPath, "youtube-dl.exe"))
                .Filter(youtubeDlPath => File.Exists(youtubeDlPath), $"youtube-dl.exe not found inside {toolsPath}")
                .Map(youtubeDlPath =>
                {
                    var youtubeDl = new YoutubeDL(youtubeDlPath);

                    youtubeDl.Options.FilesystemOptions.Output = outputPath;
                    youtubeDl.Options.PostProcessingOptions.ExtractAudio = true;
                    youtubeDl.Options.PostProcessingOptions.FfmpegLocation = ffmpegPath;
                    youtubeDl.Options.PostProcessingOptions.AudioFormat = NYoutubeDL.Helpers.Enums.AudioFormat.mp3;

                    return youtubeDl;
                });

        /// <summary>
        /// Replaces all sequences of whitespace characters with a single space. E.g. "asd     asd" becomes "asd asd".
        /// This is needed due to a bug in YoutubeDl. It often inserts multiple spaces when there is one in the video title.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>The string with sequential whitespaces removed.</returns>
        private string RemoveSequentialWhitespaces(string input) =>
            string.IsNullOrEmpty(input) ?
            input :
            Regex.Replace(input, @"\s+", " ");
    }
}