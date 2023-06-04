namespace TeamCubing.BLL.Models;

public class UserSessionDto
{
    public int Id { get; set; }

    public string Name { get; set; }

    public string UserId { get; set; }

    public List<SolveDto> Solves { get; set; }
}
