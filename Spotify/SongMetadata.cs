using Newtonsoft.Json.Linq;

public class SongMetadata
{
    public string Title { get; set; }
    public string Artist { get; set; }
    public string Album { get; }
    public string TrackNumber { get; }
    public string DiscNumber { get; }
    public string Year { get; }
    public string Duration { get; }
    public string ISRC { get; }
    public string SpotifyUrl { get; }
    public string LargestArtUrl { get; }
    public string SmallestArtUrl { get; }
    public string PreviewUrl { get; }

    public SongMetadata(string title, string artist, string album, string trackNumber,
                        string discNumber, string year, string duration, string isrc, 
                        string spotifyUrl, string largestArtUrl, string smallestArtUrl, string previewUrl)
    {
        Title = title;
        Artist = artist;
        Album = album;
        TrackNumber = trackNumber;
        DiscNumber = discNumber;
        Year = year;
        Duration = duration;
        ISRC = isrc;
        SpotifyUrl = spotifyUrl;
        LargestArtUrl = largestArtUrl;
        SmallestArtUrl = smallestArtUrl;
        PreviewUrl = previewUrl;
    }

    public static SongMetadata FromJson(string jsonResponse)
    {
        JObject json = JObject.Parse(jsonResponse);

        string title = json["name"]?.ToString() ?? "Unknown Title";
        string artist = json["artists"]?[0]?["name"]?.ToString() ?? "Unknown Artist";
        string album = json["album"]?["name"]?.ToString() ?? "Unknown Album";
        string trackNumber = json["track_number"]?.ToString() ?? "0";
        string discNumber = json["disc_number"]?.ToString() ?? "0";
        string year = json["album"]?["release_date"]?.ToString().Split('-')[0] ?? "Unknown Year";
        
        // Convert duration from milliseconds to "mm:ss" format
        int durationMs = json["duration_ms"]?.ToObject<int>() ?? 0;
        TimeSpan durationTimeSpan = TimeSpan.FromMilliseconds(durationMs);
        string duration = $"{(int)durationTimeSpan.TotalMinutes}:{durationTimeSpan.Seconds:D2}";

        string isrc = json["external_ids"]?["isrc"]?.ToString() ?? "Unknown ISRC";
        string spotifyUrl = json["external_urls"]?["spotify"]?.ToString() ?? "";

        // Get album art URLs (choose the largest and smallest available)
        JArray images = (JArray?)json["album"]?["images"] ?? new JArray();
        string largestArtUrl = images.Count > 0 ? images[0]?["url"]?.ToString() ?? "" : "";
        string smallestArtUrl = images.Count > 0 ? images[images.Count - 1]?["url"]?.ToString() ?? "" : "";

        // Preview URL (30-second audio sample)
        string previewUrl = json["preview_url"]?.ToString() ?? "";

        return new SongMetadata(title, artist, album, trackNumber, discNumber, year, duration,
                                isrc, spotifyUrl, largestArtUrl, smallestArtUrl, previewUrl);
    }

    public override string ToString()
    {
        return $"{Title} - {Artist} - {Album} - {TrackNumber}/{DiscNumber} - {Year} - {Duration} - {ISRC}";
    }

    public static SongMetadata Empty => new SongMetadata("Unknown Title", "Unknown Artist", "Unknown Album", "0", "0", "Unknown Year", "0:00", "Unknown ISRC", "", "", "", "");
}