namespace TeamCubing.API.Models;

public class SolveResponseModel
{
    public int Id { get; set; }

    public int Time { get; set; }

    public string Scramble { get; set; }

    public int SessionId { get; set; }
}
