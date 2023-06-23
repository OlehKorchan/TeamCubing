namespace TeamCubing.Domain.Settings;

public class Settings
{
    public JwtSettings JwtSettings { get; set; }

    public CosmosSettings CosmosSettings { get; set; }

    public SqlSettings SqlSettings { get; set; }
}
