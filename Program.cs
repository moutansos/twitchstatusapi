using System.Text;
using System.Text.Json;
using TwitchStatusApi;
using dotenv.net;
using System.Net.Http.Headers;
using Microsoft.Extensions.Caching.Memory;

DotEnv.Load();

IMemoryCache cache = new MemoryCache(new MemoryCacheOptions());

string clientId = Environment.GetEnvironmentVariable("TWITCH_CLIENT_ID") ?? throw new Exception("TWITCH_CLIENT_ID not set");
string clientSecret = Environment.GetEnvironmentVariable("TWITCH_CLIENT_SECRET") ?? throw new Exception("TWITCH_CLIENT_SECRET not set");

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

HttpClient client = new HttpClient();
client.DefaultRequestHeaders.Add("Client-Id", clientId);

app.MapGet("/api/status", () => (new { Status = "OK" }));

app.MapGet("/api/channels", async (int id) =>
{
    if (cache.TryGetValue<StatusPayload>(id, out StatusPayload? cachedStatus) && cachedStatus != null)
    {
        // Console.WriteLine($"Cache hit for {id}");
        return cachedStatus;
    }
    // Console.WriteLine($"Cache miss for {id}");

    string tokenRetrivalPayload = $"client_id={clientId}&client_secret={clientSecret}&grant_type=client_credentials";
    HttpResponseMessage response = await client.PostAsync($"https://id.twitch.tv/oauth2/token", new StringContent(tokenRetrivalPayload, Encoding.UTF8, "application/x-www-form-urlencoded"));
    response.EnsureSuccessStatusCode();
    string? content = await response.Content.ReadAsStringAsync();
    TwitchAuthResponse? authResponse = JsonSerializer.Deserialize<TwitchAuthResponse>(content);

    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResponse?.access_token ?? string.Empty);
    HttpResponseMessage infoResponse = await client.GetAsync($"https://api.twitch.tv/helix/streams?user_id={id}");
    infoResponse.EnsureSuccessStatusCode();
    string? infoContent = await infoResponse.Content.ReadAsStringAsync();
    TwitchStreamResponse? streamResponse = JsonSerializer.Deserialize<TwitchStreamResponse>(infoContent);

    StatusPayload status = new(
        Title: streamResponse?.data?.FirstOrDefault()?.title ?? string.Empty,
        Game: streamResponse?.data?.FirstOrDefault()?.game_name ?? string.Empty,
        Status: streamResponse?.data?.FirstOrDefault()?.type ?? "offline"
    );

    cache.Set(id, status, TimeSpan.FromMinutes(1));
    return status;
});

app.Run();
