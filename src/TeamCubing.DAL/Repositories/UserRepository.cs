using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TeamCubing.DAL.Interfaces;
using TeamCubing.Domain.Models;
using TeamCubing.Domain.Settings;

namespace TeamCubing.DAL.Repositories;

public class UserRepository : CosmosBaseRepository<ApplicationUser>, IUserRepository
{
    public UserRepository(IOptions<Settings> settings, ILogger<UserRepository> logger) : base(
        settings.Value.CosmosSettings,
        logger)
    {
    }

    public async Task<ApplicationUser> InsertAsync(ApplicationUser item)
    {
        return (await Container.CreateItemAsync(item))?.Resource;
    }

    public async Task<ApplicationUser> ReplaceAsync(ApplicationUser item)
    {
        return (await Container.ReplaceItemAsync(item, item.Id))?.Resource;
    }

    public async Task<ApplicationUser> UpsertAsync(ApplicationUser item)
    {
        return (await Container.UpsertItemAsync(item))?.Resource;
    }

    public async Task<ApplicationUser> ReadByIdAsync(string id)
    {
        return (await Container.ReadItemAsync<ApplicationUser>(id, new PartitionKey("User")))
            ?.Resource;
    }

    public async Task<ApplicationUser> ReadByNameAsync(string name)
    {
        var query =
            new QueryDefinition(
                    @"SELECT TOP 1 * FROM ApplicationUser as u WHERE u.userName = @userName")
                .WithParameter("@userName", name);

        return (await ReadFromFeedAsync(query))?.FirstOrDefault();
    }
}
