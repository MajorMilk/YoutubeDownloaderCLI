namespace SpotifyAPI;

public struct APICredentials
{
    public readonly string ClientId;
    public readonly string ClientSecret;
    public APICredentials(string clientId, string clientSecret)
    {
        ClientId = clientId;
        ClientSecret = clientSecret;
    }
    public static APICredentials Empty => new APICredentials("YOUR_CLIENT_ID", "YOUR_CLIENT_SECRET");
}