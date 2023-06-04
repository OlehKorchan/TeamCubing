namespace TeamCubing.DAL.Models;

public class Solve
{
    public int Id { get; set; }

    public int Time { get; set; }

    public string Scramble { get; set; }

    public int SessionId { get; set; }

    public Session Session { get; set; }
}
