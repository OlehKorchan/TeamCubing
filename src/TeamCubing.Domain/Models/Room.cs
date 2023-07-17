using Newtonsoft.Json;

namespace TeamCubing.Domain.Models;

public class Room
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("settings")]
    public RoomSettings Settings { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("wasOnceConnectedUserNames")]
    public List<string> WasOnceConnectedUserNames { get; set; } = new();

    [JsonProperty("connectedUserNames")]
    public List<string> ConnectedUserNames { get; set; } = new();

    [JsonProperty("password")]
    public string Password { get; set; }

    [JsonProperty("solves")]
    public List<Solve> Solves { get; set; } = new();

    [JsonProperty("cachedScrambles")]
    public List<ScrambleWithImage> CachedScrambles { get; set; } = new();
}
