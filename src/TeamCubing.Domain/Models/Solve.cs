using Newtonsoft.Json;
using TeamCubing.Domain.DTO;

namespace TeamCubing.Domain.Models;

public class Solve
{
    [JsonProperty("solveNumber")]
    public int SolveNumber { get; set; }

    [JsonProperty("scramble")]
    public string Scramble { get; set; }

    [JsonProperty("scrambledPuzzleImage")]
    public PuzzleImage ScrambledPuzzleImage { get; set; }

    [JsonProperty("startTime")]
    public DateTime StartTime { get; set; }

    [JsonProperty("results")]
    public List<SolveResult> Results { get; set; } = new();
}
