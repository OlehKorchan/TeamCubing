namespace TeamCubing.Domain.Settings;

public class JwtSettings
{
    public string TokenKey { get; set; }

    public int ExpiresInHours { get; set; }
}
