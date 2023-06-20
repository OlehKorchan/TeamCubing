using Newtonsoft.Json;

namespace TeamCubing.Domain.Models;

public class Solve
{
    [JsonProperty("solveNumber")]
    public int SolveNumber { get; set; }

    [JsonProperty("scramble")]
    public string Scramble { get; set; }

    [JsonProperty("startTime")]
    public DateTime StartTime { get; set; }

    [JsonProperty("results")]
    public List<SolveResult> Results { get; set; } = new();
}
