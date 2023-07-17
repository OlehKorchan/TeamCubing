using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using TeamCubing.Domain.Settings;

namespace TeamCubing.DAL.Repositories;

public abstract class CosmosBaseRepository<T>
{
    private readonly ILogger<CosmosBaseRepository<T>> _logger;
    private readonly CosmosSettings _cosmosSettings;
    protected Container Container;

    protected CosmosBaseRepository(
        CosmosSettings cosmosSettings,
        ILogger<CosmosBaseRepository<T>> logger)
    {
        _cosmosSettings = cosmosSettings;
        _logger = logger;
        InitializeAsync().GetAwaiter().GetResult();
    }

    protected async Task InitializeAsync()
    {
        var cosmosClient = new CosmosClient(_cosmosSettings.Host, _cosmosSettings.Secret);

        var containerOptions = new ContainerProperties
        {
            Id = typeof(T).Name,
            PartitionKeyPath = "/id",
        };

        var database = (await cosmosClient.CreateDatabaseIfNotExistsAsync(_cosmosSettings.Database))
            .Database;

        Container = (await database.CreateContainerIfNotExistsAsync(containerOptions))
            .Container;
    }

    protected async Task<List<T>> ReadFromFeedAsync(QueryDefinition query)
    {
        var entities = new List<T>();

        try
        {
            using var feed = Container.GetItemQueryIterator<T>(query);

            while (feed.HasMoreResults)
            {
                var response = await feed.ReadNextAsync();
                entities.AddRange(response);
            }
        }
        catch (CosmosException e)
        {
            _logger.LogWarning("Cosmos error occured: {Message}", e.Message);
        }

        return entities;
    }
}
