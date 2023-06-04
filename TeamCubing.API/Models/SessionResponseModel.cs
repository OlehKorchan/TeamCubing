namespace TeamCubing.API.Models;

public class SessionResponseModel
{
    public int Id { get; set; }

    public string Name { get; set; }

    public List<SolveResponseModel> Solves { get; set; }
}
