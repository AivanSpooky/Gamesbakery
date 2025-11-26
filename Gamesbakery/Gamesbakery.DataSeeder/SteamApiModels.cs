using System.Text.Json.Serialization;

public class SteamAppListResponse
{
    [JsonPropertyName("applist")]
    public SteamAppList Applist { get; set; }
}

public class SteamAppList
{
    [JsonPropertyName("apps")]
    public List<SteamApp> Apps { get; set; }
}

public class SteamApp
{
    [JsonPropertyName("appid")]
    public int Appid { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }
}

public class SteamAppDetails
{
    public bool Success { get; set; }
    public SteamAppData Data { get; set; }
}

public class SteamAppData
{
    public List<SteamGenre> Genres { get; set; }
}

public class SteamGenre
{
    public string Description { get; set; }
}
