namespace TeamCubing.API.Models;

public class SolveRequestModel
{
    public int Id { get; set; }

    public int Time { get; set; }

    public string Scramble { get; set; }

    public int SessionId { get; set; }
}
