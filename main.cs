using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SpotifyNowPlaying
{
    class Program
    {
        private const string clientId = "SPOTIFY BOT ID";
        private const string clientSecret = "SPOTIFY BOT SECRET";
        private const string redirectUri = "http://localhost:5000/callback";
        private const string authorizeUrl = "https://accounts.spotify.com/authorize";
        private const string tokenUrl = "https://accounts.spotify.com/api/token";
        private const string nowPlayingUrl = "https://api.spotify.com/v1/me/player/currently-playing";

        static async Task Main(string[] args)
        {
            var listener = new HttpListener();
            listener.Prefixes.Add("http://localhost:5000/");
            listener.Start();
            Console.WriteLine("Please visit the following URL to authorize the application:");
            Console.WriteLine($"{authorizeUrl}?client_id={clientId}&response_type=code&redirect_uri={redirectUri}&scope=user-read-currently-playing");
            Console.WriteLine("Waiting for authorization...");
            var context = await listener.GetContextAsync();
            var code = context.Request.QueryString["code"];
            var tokenResponse = await ExchangeAuthorizationCodeForToken(code);
            var accessToken = tokenResponse.AccessToken;
            var lastPlayedTrackId = "";
            while (true)
            {
                var nowPlayingResponse = await GetCurrentlyPlayingTrack(accessToken);
                if (nowPlayingResponse.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var nowPlayingData = JsonConvert.DeserializeObject<NowPlayingResponse>(await nowPlayingResponse.Content.ReadAsStringAsync());
                    var currentTrackId = nowPlayingData.Item?.Id;
                    if (!string.IsNullOrEmpty(currentTrackId) && currentTrackId != lastPlayedTrackId)
                    {
                        Console.WriteLine($"Currently Playing: {nowPlayingData.Item.Name} by {nowPlayingData.Item.Artists[0].Name}");
                        var realdeak = $"Currently Playing On Spotify: {nowPlayingData.Item.Name} by {nowPlayingData.Item.Artists[0].Name}\n\nᴘᴏᴡᴇʀᴇᴅ ʙʏ ʀʙx ᴘʀᴇꜱᴇɴᴄᴇ [ʀᴇᴀʟ ᴛɪᴍᴇ ʀᴏʙʟᴏx ꜱᴛᴀᴛᴜꜱ]";
                        var url = "https://auth.roblox.com/";
                        var httpRequest = (HttpWebRequest)WebRequest.Create(url);
                        httpRequest.Method = "POST";
                        var robloxc = "ROBLOX COOKIE";
                        httpRequest.Headers["cookie"] = ".ROBLOSECURITY=" + robloxc;
                        httpRequest.ContentType = "application/json";
                        httpRequest.Headers["Content-Length"] = "0";

                        try
                        {
                            var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
                            Console.WriteLine("Response headers:");
                            Console.WriteLine(httpResponse.Headers["x-csrf-token"]);
                        }
                        catch (WebException e)
                        {
                            var httpResponse = (HttpWebResponse)e.Response;
                            var token = httpResponse.Headers["x-csrf-token"];
                            string newDescription = realdeak;

                            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://accountinformation.roblox.com/v1/description");

                            request.Method = "POST";
                            request.ContentType = "application/json";
                            request.Headers.Add("Cookie", ".ROBLOSECURITY=" + robloxc);
                            request.Headers["x-csrf-token"] = token;

                            string json = "{\"description\":\"" + newDescription + "\"}";

                            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(json);

                            request.ContentLength = bytes.Length;

                            using (Stream requestStream = request.GetRequestStream())
                            {
                                requestStream.Write(bytes, 0, bytes.Length);
                            }

                            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                            {
                                if (response.StatusCode == HttpStatusCode.OK)
                                {
                                    Console.WriteLine("Account description updated successfully!");
                                }
                                else
                                {
                                    Console.WriteLine("Failed to update account description. Response code: " + response.StatusCode);
                                }
                            }
                        }
                        lastPlayedTrackId = currentTrackId;
                    }
                }
                else if (nowPlayingResponse.StatusCode == System.Net.HttpStatusCode.NoContent)
                {
                    Console.WriteLine("User is not currently playing a track.");
                    lastPlayedTrackId = "";
                }
                else
                {
                    Console.WriteLine($"An error occurred: {nowPlayingResponse.StatusCode}");
                }
                await Task.Delay(5000);
            }
        }


        static async Task<TokenResponse> ExchangeAuthorizationCodeForToken(string code)
        {
            using var client = new HttpClient();

            var request = new HttpRequestMessage(HttpMethod.Post, tokenUrl);
            request.Headers.Add("Authorization", $"Basic {Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"))}");
            request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "authorization_code",
                ["code"] = code,
                ["redirect_uri"] = redirectUri
            });

            var response = await client.SendAsync(request);

            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<TokenResponse>(responseJson);
        }

        static async Task<HttpResponseMessage> GetCurrentlyPlayingTrack(string accessToken)
        {
            using var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, nowPlayingUrl);
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
            return await client.SendAsync(request);
        }
    }

    public class NowPlayingResponse
    {
        public Item Item { get; set; }
    }

    public class Item
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public List<Artist> Artists { get; set; }
    }

    public class Artist
    {
        public string Name { get; set; }
    }

    public class TokenResponse
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("token_type")]
        public string TokenType { get; set; }

        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }

        [JsonProperty("scope")]
        public string Scope { get; set; }
    }
}


