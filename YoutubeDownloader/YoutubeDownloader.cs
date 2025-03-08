using YoutubeExplode;
using Newtonsoft.Json;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;
using CliWrap;

internal class YoutubeDownloader
{
    private static readonly YoutubeClient Client = new YoutubeClient();
    private static bool _videoMode;
    public static async Task Main(string[] args)
    {
        string link;
        try // bad way to handle arguments, but whatever. If someone wanted to implement CommandLineParser, I would not be opposed.
        {
            link = args[0];

            try
            {
                _ = args[1];
                _videoMode = true;
            } 
            catch (IndexOutOfRangeException)
            { }
        }
        catch (IndexOutOfRangeException)
        {
            Console.WriteLine("To use this program, you must provide a link to a video.");
            return;
        }
        if (link.Contains("youtube.com/watch?v="))
        {
            var v = await GetVideoAsync(link);
            await DownloadAsync(link, v);
        }

    }

    private static async Task<Video> GetVideoAsync(string link)
    {
        var video = await Client.Videos.GetAsync(link);
        return video;
    }

    private static string ReplaceInvalidChars(string filename)
    {
        return string.Join("_", filename.Split(Path.GetInvalidFileNameChars()));    
    }

    private static async Task DownloadAsync(string link, Video video)
{
    Console.WriteLine($"Downloading {video.Title}");
    var streamManifest = await Client.Videos.Streams.GetManifestAsync(link);
    var audioStreamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();
    
    string fname = ReplaceInvalidChars(video.Title);
    string json = JsonConvert.SerializeObject(video, Formatting.Indented);
    var downloadsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
    downloadsPath = Path.Combine(downloadsPath, "YoutubeDownloads");

    if (!Directory.Exists(downloadsPath))
    {
        Directory.CreateDirectory(downloadsPath);
    }

    if (!Directory.Exists(Path.Combine(downloadsPath, "jsons")))
    {
        Directory.CreateDirectory(Path.Combine(downloadsPath, "jsons"));
    }

    File.WriteAllText(Path.Combine(Path.Combine(downloadsPath, "jsons"), $"{fname}.json"), json);

    var audioFileName = Path.Combine(downloadsPath, $"{fname}.{audioStreamInfo.Container}");
    await Client.Videos.Streams.DownloadAsync(audioStreamInfo, audioFileName);

    if (_videoMode) // video files from youtube do not come muxed, so it needs to be downloaded separately
    {
        var videoStreamInfo = streamManifest.GetVideoOnlyStreams().GetWithHighestBitrate();
        var videoPath = Path.Combine(downloadsPath, "Videos");
        if (!Directory.Exists(videoPath))
        {
            Directory.CreateDirectory(videoPath);
        }
        var videoFilename = Path.Combine(videoPath, $"{fname}.{videoStreamInfo.Container}");
        await Client.Videos.Streams.DownloadAsync(videoStreamInfo, videoFilename);

        Console.WriteLine("Download Complete, beginning conversion...");
        var newVideoFileName = videoFilename.Replace(".webm", ".mp4");

        // Print the exact command for debugging
        string ffmpegCommand = $"ffmpeg -i \"{videoFilename}\" -i \"{audioFileName}\" -c:v libx264 -c:a aac \"{newVideoFileName}\"";
        Console.WriteLine($"Executing: {ffmpegCommand}");

        try
        {
            var result = await Cli.Wrap("ffmpeg")
                .WithArguments($"-i \"{videoFilename}\" -i \"{audioFileName}\" -c:v libx264 -c:a aac \"{newVideoFileName}\"")
                .WithValidation(CommandResultValidation.None) // Disable validation to capture errors
                .ExecuteAsync();

            if (result.ExitCode != 0)
            {
                Console.WriteLine($"FFmpeg failed with exit code {result.ExitCode}");
            }
            else
            {
                Console.WriteLine("Conversion completed successfully!");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine("Conversion failed. Check the file paths and FFmpeg installation.");
        }

        // Cleanup
        File.Delete(videoFilename);
        File.Delete(audioFileName);
        Console.WriteLine("All Done");
    }
    else // for audio only conversion
    {
        Console.WriteLine("Download Complete, beginning conversion...");

        var newFileName = audioFileName.Replace(".webm", ".m4a");
        var webmPath = Path.Combine(downloadsPath, "webm");

        if (!Directory.Exists(webmPath))
        {
            Directory.CreateDirectory(webmPath);
        }

        // Print the exact command for debugging
        string ffmpegCommand = $"-i \"{audioFileName}\" -vn -c:a aac -b:a 192k -y \"{newFileName}\""; // ffprobe says the bitrate is closer to 130kbps, that's why I lowered it from 320'
        Console.WriteLine($"Executing: {ffmpegCommand}");

        try
        {
            var result = await Cli.Wrap("ffmpeg")
                .WithArguments(ffmpegCommand) 
                .WithValidation(CommandResultValidation.None) // Disable validation to capture errors
                .ExecuteAsync();

            if (result.ExitCode != 0)
            {
                Console.WriteLine($"FFmpeg failed with exit code {result.ExitCode}");
            }
            else
            {
                Console.WriteLine("Conversion completed successfully!");
                Console.WriteLine("Adding metadata to file...");
 
                var res = await Cli.Wrap("YoutubeMetadataGenerator").WithArguments($"\"{newFileName}\"")
                    .WithStandardOutputPipe(PipeTarget.ToDelegate(Console.WriteLine))
                    .WithStandardErrorPipe(PipeTarget.ToDelegate(Console.Error.WriteLine))
                    .ExecuteAsync();
            }

            File.Move(audioFileName, Path.Combine(webmPath, $"{fname}.webm"));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine("Conversion failed. Do you have FFMPEG installed and in your PATH?");
        }
    }
}

}
