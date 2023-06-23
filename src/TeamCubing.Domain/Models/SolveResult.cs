using Newtonsoft.Json;

namespace TeamCubing.Domain.Models;

public class SolveResult
{
    [JsonProperty("userName")]
    public string UserName { get; set; }

    [JsonProperty("time")]
    public int Time { get; set; }

    [JsonProperty("penalty")]
    public Penalty Penalty { get; set; }
}
