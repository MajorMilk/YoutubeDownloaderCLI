using Newtonsoft.Json;
using CliWrap;
using YoutubeExplode.Common;
using YoutubeExplode.Videos;

internal class YoutubeMetadataGenerator
{
    private static async Task ApplyToAllFiles()
    {
        var downloadsDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        downloadsDir = Path.Combine(downloadsDir, "Downloads");
        downloadsDir = Path.Combine(downloadsDir, "YoutubeDownloads");
        var files = Directory.GetFiles(downloadsDir);

        foreach (var file in files)
        {
            string filename = Path.GetFileName(file);
            // Get the folder containing the file
            string parentOfCurrentFolder = Path.GetDirectoryName(file);

            // Get the parent directory of the folder containing the file
            //string parentOfCurrentFolder = Directory.GetParent(currentFolder)?.FullName;

            var jsonsFolder = Path.Combine(parentOfCurrentFolder, "jsons");

            var jsonFileName = filename.Replace(".m4a", ".json");

            var json = File.ReadAllText(Path.Combine(jsonsFolder, jsonFileName));

            var video = JsonConvert.DeserializeObject<Video>(json);


            string Title = video.Title;

            //This is to fix ' - Topic' being added to artist metadata.
            //"topic" channels are created automatically when music publisher uploads your music.
            //This is the highest quality recording you can find on YouTube, since it's generated using the original files. 
            string Author = video.Author.ChannelTitle.Replace(" - Topic", "");
            
            string artUrl = video.Thumbnails[^1].Url;

            string savePath = Path.Combine(jsonsFolder, $"thumbnails");

            if (!Directory.Exists(savePath))
            {
                Directory.CreateDirectory(savePath);
            }

            savePath = Path.Combine(savePath, $"{Path.GetFileNameWithoutExtension(file)}.jpg");

            using (HttpClient httpClient = new HttpClient())
            {
                try
                {
                    byte[] imageBytes = await httpClient.GetByteArrayAsync(artUrl);

                    await File.WriteAllBytesAsync(savePath, imageBytes);

                    Console.WriteLine($"Image downloaded and saved to {savePath}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred: {ex.Message}");
                }

            }

            var tempFile = Path.GetFileNameWithoutExtension(file);
            tempFile = file.Replace(tempFile, "temp");
            tempFile = Path.Combine(downloadsDir, tempFile);
            //ffmpeg -i input.m4a -i cover.jpg -map 0 -map 1 -c copy -metadata title="Song Title" -metadata artist="Artist Name" -disposition:v attached_pic output.m4a
            string command =
                $"-i \"{file}\" -i \"{savePath}\" -map 0 -map 1 -c copy -metadata title=\"{Title}\" -metadata artist=\"{Author}\" -disposition:v attached_pic \"{tempFile}\"";

            Console.WriteLine("Executing: " + command);

            try
            {
                var result = await Cli
                    .Wrap("ffmpeg") // For some reason, this doesnt work for all files... I think It might have something to do with the filename? Still works for most files tho
                    .WithArguments(command)
                    .WithValidation(CommandResultValidation.None) // Disable validation to capture errors
                    .WithStandardOutputPipe(PipeTarget.ToDelegate(Console.WriteLine)) // Forward stdout // Uncomment these things if u want to see FFmpeg spam the console
                    .WithStandardErrorPipe(PipeTarget.ToDelegate(Console.Error.WriteLine)) // Forward stderr
                    .ExecuteAsync();

                if (result.ExitCode != 0)
                {
                    Console.WriteLine($"FFmpeg failed with exit code {result.ExitCode}");
                }
                else
                {
                    Console.WriteLine("FFmpeg completed successfully");
                    File.Delete(file);
                    File.Move(tempFile, file);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }

    private static async Task Main(string[] args)
    {
        try
        {
            string filePath = args[0];
            
            var downloadsDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            downloadsDir = Path.Combine(downloadsDir, "Downloads");
            downloadsDir = Path.Combine(downloadsDir, "YoutubeDownloads");
            
            var jsonDirPath = Path.Combine(downloadsDir, "jsons");
            
            string filename = Path.GetFileName(filePath);

            if (!File.Exists(filePath))
            {
                Console.WriteLine("File not found: " + filePath);
                return;
            }
            else
            {
                var jsonFileName = filename.Replace(".m4a", ".json");
                var json = File.ReadAllText(Path.Combine(jsonDirPath, jsonFileName));
                var video = JsonConvert.DeserializeObject<Video>(json);
                string Title = video.Title;
                Author Author = video.Author;
                string artUrl = video.Thumbnails[^1].Url;
                string savePath = Path.Combine(jsonDirPath, "thumbnails");
                savePath = Path.Combine(savePath, $"{Path.GetFileNameWithoutExtension(filePath)}.jpg");
                
                using (HttpClient httpClient = new HttpClient())
                {
                    try
                    {
                        byte[] imageBytes = await httpClient.GetByteArrayAsync(artUrl);

                        await File.WriteAllBytesAsync(savePath, imageBytes);

                        Console.WriteLine($"Image downloaded and saved to {savePath}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"An error occurred: {ex.Message}");
                    }

                }
                
                var tempFile = Path.GetFileNameWithoutExtension(filePath);
                tempFile = filePath.Replace(tempFile, "temp");
                tempFile = Path.Combine(downloadsDir, tempFile);
                //ffmpeg -i input.m4a -i cover.jpg -map 0 -map 1 -c copy -metadata title="Song Title" -metadata artist="Artist Name" -disposition:v attached_pic output.m4a
                string command =
                    $"-i \"{filePath}\" -i \"{savePath}\" -map 0 -map 1 -c copy -metadata title=\"{Title}\" -metadata artist=\"{Author.ChannelTitle}\" -disposition:v attached_pic \"{tempFile}\"";

                Console.WriteLine("Executing: ffmpeg " + command);

                try
                {
                    var result = await Cli
                        .Wrap("ffmpeg") // For some reason, this doesnt work for all files... I think It might have something to do with the filename? Still works for most files tho
                        .WithArguments(command)
                        .WithValidation(CommandResultValidation.None) // Disable validation to capture errors
                        //.WithStandardOutputPipe(PipeTarget.ToDelegate(Console.WriteLine)) // Forward stdout // Uncomment these things if u want to see FFmpeg spam the console
                        //.WithStandardErrorPipe(PipeTarget.ToDelegate(Console.Error.WriteLine)) // Forward stderr
                        .ExecuteAsync();

                    if (result.ExitCode != 0)
                    {
                        Console.WriteLine($"FFmpeg failed with exit code {result.ExitCode}");
                    }
                    else
                    {
                        Console.WriteLine("FFmpeg completed successfully");
                        File.Delete(filePath);
                        File.Move(tempFile, filePath);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
                
                
            }
            
        }
        catch (IndexOutOfRangeException)
        {
            Console.WriteLine("Are you sure you want to apply this to all downloaded files? (y/n)");
            
            var response = Console.ReadLine();

            if (response != "y")
            {
                return;
            }
            await ApplyToAllFiles();
        }
        
    }
}