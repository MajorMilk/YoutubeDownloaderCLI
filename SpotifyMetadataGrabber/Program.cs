using System.Reflection;
using CliWrap;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SpotifyAPI;
using YoutubeExplode.Videos;


class Program
{
    public static async Task Main(string[] args)
    {
        string downloadsPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        downloadsPath = Path.Combine(downloadsPath, "Downloads");
        downloadsPath = Path.Combine(downloadsPath, "YoutubeDownloads");
        
        string jsonDirPath = Path.Combine(downloadsPath, "jsons");

        try
        {
            string file = args[0];
            if (!File.Exists(file))
            {
                Console.WriteLine($"File not found: {file}");
                Environment.Exit(1);
            }
            
            string jsonFileName = Path.GetFileName(file).Replace(".m4a", ".json");
            string jsonPath = Path.Combine(jsonDirPath, jsonFileName);
            SongMetadata song;
            
            string title;
            string author;
            
            if (!File.Exists(jsonPath))
            {
                Console.WriteLine($"No JSON file found for {jsonPath}. Searching for song metadata base don incomplete info...");
                song = await Spotify.Instance.SearchSong(Path.GetFileNameWithoutExtension(file), "Unknown");
            }
            else
            {
                string json = File.ReadAllText(jsonPath);
                Video v = JsonConvert.DeserializeObject<Video>(json);
                title = v.Title;
                author = v.Author.ChannelTitle.Replace(" - Topic", "");
                song = await Spotify.Instance.SearchSong(title, author);
            }
            

            if (song.Album != "Unknown Album")
            {
                string filenameWOE = Path.GetFileNameWithoutExtension(file);
                string tempFileName = file.Replace(filenameWOE, $"{filenameWOE}temp");
                
                string imageDir = Path.Combine(jsonDirPath, "thumbnails");
                string imagePath = Path.Combine(imageDir, $"{filenameWOE}.jpg");

                
                string command =
                    $"-i \"{file}\" -i \"{imagePath}\" -map 0:a -map 1:v -c:a copy -c:v copy -disposition:v:0 attached_pic -metadata title=\"{song.Title}\" " +
                    $"-metadata artist=\"{song.Artist}\" " +
                    $"-metadata album=\"{song.Album}\" " +
                    $"-metadata track=\"{song.TrackNumber}\" " +
                    $"-metadata disc=\"{song.DiscNumber}\" " +
                    $"-metadata year=\"{song.Year}\" " +
                    $"-metadata isrc=\"{song.ISRC}\" " +
                    $"-metadata comment=\"Downloaded from Youtube using YoutubeDownloaderCLI - Metadata from Spotify\" \"{tempFileName}\"";  // Output file in .m4a format
                
                var result = await Cli.Wrap("ffmpeg").WithArguments(command).WithStandardErrorPipe(PipeTarget.ToDelegate(Console.Error.WriteLine)).ExecuteAsync();
                
                if (result.ExitCode != 0)
                {
                    Console.WriteLine($"FFmpeg failed with exit code {result.ExitCode}");
                }
                else
                {
                    File.Delete(file);
                    File.Move(tempFileName, file);
                    Console.WriteLine("Metadata updated successfully!");
                    
                }

            } 
            return;
        }
        catch (IndexOutOfRangeException)
        {
            Console.WriteLine("Are you sure you want to apply this to all downloaded files? (y/n)");
            if (Console.ReadLine() != "y") return;
        }
        
        var files = Directory.GetFiles(downloadsPath);
        foreach (var file in files)
        {
            string filename = Path.GetFileName(file);
            string jsonFileName = filename.Replace(".m4a", ".json");
            string jsonPath = Path.Combine(jsonDirPath, jsonFileName);
            SongMetadata song;
            
            string title;
            string author;
            
            if (!File.Exists(jsonPath))
            {
                song = await Spotify.Instance.SearchSong(Path.GetFileNameWithoutExtension(file), "Unknown Artist");
            }
            else
            {
                string json = File.ReadAllText(jsonPath);
                Video v = JsonConvert.DeserializeObject<Video>(json);
                title = v.Title;
                author = v.Author.ChannelTitle.Replace(" - Topic", "");
                song = await Spotify.Instance.SearchSong(title, author);
            }
            

            if (song.Album != "Unknown Album")
            {
                string filenameWOE = Path.GetFileNameWithoutExtension(file);
                string tempFileName = file.Replace(filenameWOE, $"{filenameWOE}temp");
                
                string imageDir = Path.Combine(jsonDirPath, "thumbnails");
                string imagePath = Path.Combine(imageDir, $"{filenameWOE}.jpg");

                
                string command =
                    $"-i \"{file}\" -i \"{imagePath}\" -map 0:a -map 1:v -c:a copy -c:v copy -disposition:v:0 attached_pic -metadata title=\"{song.Title}\" -metadata artist=\"{song.Artist}\" -metadata album=\"{song.Album}\" -metadata track=\"{song.TrackNumber}\" -metadata disc=\"{song.DiscNumber}\" -metadata year=\"{song.Year}\" -metadata isrc=\"{song.ISRC}\" -metadata comment=\"Downloaded from Youtube using YoutubeDownloaderCLI - Metadata from Spotify\" \"{tempFileName}\"";  // Output file in .m4a format
                
                var result = await Cli.Wrap("ffmpeg").WithArguments(command).WithStandardErrorPipe(PipeTarget.ToDelegate(Console.Error.WriteLine)).ExecuteAsync();
                
                if (result.ExitCode != 0)
                {
                    Console.WriteLine($"FFmpeg failed with exit code {result.ExitCode}");
                }
                else
                {
                    File.Delete(file);
                    File.Move(tempFileName, file);
                    Console.WriteLine("Metadata updated successfully!");
                }

            }
            
        }
    }
}


class Spotify
{
    private static readonly Lazy<Spotify> _instance = new Lazy<Spotify>(() => new Spotify());
    public static Spotify Instance => _instance.Value;

    private readonly string clientId;
    private readonly string clientSecret;
    private readonly string tokenUrl = "https://accounts.spotify.com/api/token";
    private readonly string searchUrl = "https://api.spotify.com/v1/search";

    private string _accessToken;
    private DateTime _tokenExpiration;
    private readonly HttpClient _httpClient;
    private readonly SemaphoreSlim _tokenSemaphore = new SemaphoreSlim(1, 1); 

    private Spotify()
    {
        string downloadsPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        downloadsPath = Path.Combine(downloadsPath, "Downloads");
        downloadsPath = Path.Combine(downloadsPath, "YoutubeDownloads");
        string configPath = Path.Combine(Path.Combine(Path.Combine(downloadsPath, "jsons")), "SpotifyAPICreds.json");

        if (!File.Exists(configPath))
        {
            Console.WriteLine("SpotifyAPICreds.json not found...\n\nPlease add your client ID and client secret to SpotifyAPICreds.json.");
            string json = JsonConvert.SerializeObject(APICredentials.Empty, Formatting.Indented);
            File.WriteAllText(configPath, json);
            Environment.Exit(1);
        }

        var creds = JsonConvert.DeserializeObject<APICredentials>(File.ReadAllText(configPath));

        if (creds.ClientId == "YOUR_CLIENT_ID" || creds.ClientSecret == "YOUR_CLIENT_SECRET")
        {
            Console.WriteLine("SpotifyAPICreds.json is not configured correctly...\n\nPlease add your client ID and secret.");
            Environment.Exit(1);
        }

        clientId = creds.ClientId;
        clientSecret = creds.ClientSecret;
        _httpClient = new HttpClient();
    }

    private async Task<string> GetAccessToken()
    {
        await _tokenSemaphore.WaitAsync();
        try
        {
            if (!string.IsNullOrEmpty(_accessToken) && _tokenExpiration > DateTime.UtcNow)
            {
                return _accessToken;
            }

            var request = new HttpRequestMessage(HttpMethod.Post, tokenUrl);
            request.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(
                System.Text.Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}")));

            request.Content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "client_credentials")
            });

            HttpResponseMessage response = await _httpClient.SendAsync(request);
            string responseContent = await response.Content.ReadAsStringAsync();
            JObject json = JObject.Parse(responseContent);

            _accessToken = json["access_token"].ToString();
            _tokenExpiration = DateTime.UtcNow.AddSeconds(Convert.ToInt32(json["expires_in"]) - 60);

            return _accessToken;
        }
        finally
        {
            await Task.Delay(500);
            _tokenSemaphore.Release();
        }
    }

    public async Task<SongMetadata> SearchSong(string title, string author)
    {
        string token = await GetAccessToken();
        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);

        string query = Uri.EscapeDataString($"track:{title} artist:{author}");
        string url = $"{searchUrl}?q={query}&type=track&limit=1";

        HttpResponseMessage response = await _httpClient.GetAsync(url);
        string responseContent = await response.Content.ReadAsStringAsync();
        JObject json = JObject.Parse(responseContent);

        var track = json["tracks"]?["items"]?.First;
        if (track != null)
        {
            return SongMetadata.FromJson(track.ToString());
        }
        else
        {
            Console.WriteLine($"No results found for {title} - {author}.");
            var md = SongMetadata.Empty;
            md.Title = title;
            md.Artist = author;
            return md;
        }
    }
}

