using Newtonsoft.Json;

namespace TeamCubing.Domain.Models;

public class ApplicationUser : ICosmosModel
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("userName")]
    public string UserName { get; set; }

    [JsonProperty("passwordHash")]
    public string PasswordHash { get; set; }

    [JsonProperty("passwordSalt")]
    public byte[] PasswordSalt { get; set; }

    [JsonProperty("lastRoomName")]
    public string LastRoomName { get; set; }

    [JsonProperty("solves")]
    public List<UserSolve> Solves { get; set; } = new();

    [JsonProperty(nameof(PartitionKey))]
    public string PartitionKey { get; set; } = nameof(ApplicationUser);
}
