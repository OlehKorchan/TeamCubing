namespace TeamCubing.API.Models;

public class LoginResponseModel : ResponseModelBase
{
    public string Username { get; set; }

    public string Token { get; set; }

    public int ExpiresIn { get; set; }
}
