namespace TeamCubing.Domain.ResponseModels;

public class LoginResponseModel : BaseResponse
{
    public string Username { get; set; }

    public string Token { get; set; }

    public int ExpiresIn { get; set; }
}
