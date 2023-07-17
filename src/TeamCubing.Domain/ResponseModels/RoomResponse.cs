using Newtonsoft.Json;
using TeamCubing.Domain.Models;

namespace TeamCubing.Domain.ResponseModels;

public class RoomResponse
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("settings")]
    public RoomSettings Settings { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("administratorName")]
    public string AdministratorName { get; set; }

    [JsonProperty("wasOnceConnectedUserNames")]
    public List<string> WasOnceConnectedUserNames { get; set; } = new();

    [JsonProperty("connectedUserNames")]
    public List<string> ConnectedUserNames { get; set; } = new();

    [JsonProperty("solves")]
    public List<SolveResponse> Solves { get; set; } = new();
}
