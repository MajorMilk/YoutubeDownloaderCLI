using CliWrap;
using YoutubeExplode;


internal class PlaylistDownloader
{
    private static async Task Main(string[] args)
    {

        var url = "";
        try
        {
            url = args[0];
            if (url.Contains("playlist?list="))
            {
                url = url.Split("playlist?list=")[^1];
            }
            else
            { // On linux, Im running into issues with bash recognizing & I think.
                Console.WriteLine("You need to provide the full link to a playlist, preferably in quotes.");
            }
        }
        catch (IndexOutOfRangeException)
        {
            Console.WriteLine("You need to provide a link to a playlist.");
            throw;
        }
        
        var Client = new YoutubeClient();
        
        var playlist = await Client.Playlists.GetAsync(url);

        Console.WriteLine($"Playlist: {playlist.Title}\nNumber of videos: {playlist.Count}\n\nAre you sure you want to download this playlist? (y/n)");
        var response = Console.ReadLine();
        if (response != "y")
        {
            return;
        }
        
        await foreach (var video in Client.Playlists.GetVideosAsync(url))
        {
            try
            {
                //var result = await Cli.Wrap("YoutubeDownloader").WithArguments($"\"{video.Url}\"").ExecuteAsync();
                
                var command = Cli.Wrap("YoutubeDownloader")
                    .WithArguments($"\"{video.Url}\"");

                // Forward standard output and error to the console
                var result = await command
                    .WithStandardOutputPipe(PipeTarget.ToDelegate(Console.WriteLine)) // Forward stdout
                    .WithStandardErrorPipe(PipeTarget.ToDelegate(Console.Error.WriteLine)) // Forward stderr
                    .ExecuteAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

    }
}
