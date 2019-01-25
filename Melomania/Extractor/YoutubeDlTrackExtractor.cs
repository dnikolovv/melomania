using Melomania.Music;
using NYoutubeDL;
using Optional;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Melomania.Extractor
{
    public class YoutubeDlTrackExtractor : ITrackExtractor
    {
        public YoutubeDlTrackExtractor(string toolsFolder, string tempFolder)
        {
            _toolsFolder = toolsFolder;
            _tempFolder = tempFolder;
        }

        private readonly string _toolsFolder;
        private readonly string _tempFolder;

        public async Task<Option<Track, Error>> ExtractTrackFromUrl(string url, string fileName)
        {
            try
            {
                // TODO: Ffmpeg should be embedded or stored in the config folder
                // TODO: Perhaps download the tools the first time you run the application?
                // TODO: If the fileName is null then use the video name
                var youtubeDlPath = Path.Combine(_toolsFolder, "youtube-dl.exe");
                var ffmpegLocation = _toolsFolder;
                var fileLocation = Path.Combine(_tempFolder, $"{fileName}.mp3");
                var youtubeDl = new YoutubeDL(youtubeDlPath);

                youtubeDl.Options.FilesystemOptions.Output = fileLocation;
                youtubeDl.Options.PostProcessingOptions.ExtractAudio = true;
                youtubeDl.VideoUrl = url;
                youtubeDl.Options.PostProcessingOptions.FfmpegLocation = ffmpegLocation;
                youtubeDl.Options.PostProcessingOptions.AudioFormat = NYoutubeDL.Helpers.Enums.AudioFormat.mp3;

                await youtubeDl.PrepareDownloadAsync();

                youtubeDl.Info.PropertyChanged += (sender, args) => Console.WriteLine($"Property changed event: {args.PropertyName}");

                await youtubeDl.DownloadAsync();

                // TODO: Use either
                // TODO: The temp track should be deleted after downloading
                var trackContents = File.OpenRead(fileLocation);

                return new Track
                {
                    Contents = trackContents,
                    Name = fileName
                }.Some<Track, Error>();
            }
            catch (Exception e)
            {
                throw;
            }
        }
    }
}
