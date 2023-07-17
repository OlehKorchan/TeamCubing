using Newtonsoft.Json;
using TeamCubing.Domain.DTO;
using TeamCubing.Domain.Models;

namespace TeamCubing.Domain.ResponseModels;

public class SolveResponse
{
    [JsonProperty("solveNumber")]
    public int SolveNumber { get; set; }

    [JsonProperty("scramble")]
    public string Scramble { get; set; }

    [JsonProperty("scrambledPuzzleImage")]
    public PuzzleImage ScrambledPuzzleImage { get; set; }

    [JsonProperty("results")]
    public List<SolveResult> Results { get; set; } = new();
}
