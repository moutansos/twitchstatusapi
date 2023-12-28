namespace TwitchStatusApi;

public record TwitchAuthResponse(string access_token, int expires_in, string token_type);
public record TwitchStreamResponse(List<TwitchStream> data);
public record TwitchStream(string id, string user_id, string user_name, string game_id, string game_name, string type, string title, int viewer_count, string started_at, string language, string thumbnail_url);
public record StatusPayload(string Title, string Game, string Status);
