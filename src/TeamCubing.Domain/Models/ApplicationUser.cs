using Newtonsoft.Json;

namespace TeamCubing.Domain.Models;

public class ApplicationUser
{
    [JsonProperty("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonProperty("userName")]
    public string UserName { get; set; }

    [JsonProperty("passwordHash")]
    public string PasswordHash { get; set; }

    [JsonProperty("passwordSalt")]
    public byte[] PasswordSalt { get; set; }

    [JsonProperty("lastRoomName")]
    public string LastRoomName { get; set; }
}
