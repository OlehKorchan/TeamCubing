using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using TeamCubing.DAL.Interfaces;
using TeamCubing.Domain.Models;
using TeamCubing.Domain.Settings;

namespace TeamCubing.DAL.Repositories;

public class RoomRepository : IRoomRepository
{
    private readonly CosmosSettings _cosmosSettings;
    private Container _roomsContainer;

    public RoomRepository(IOptions<Settings> settings)
    {
        _cosmosSettings = settings.Value.CosmosSettings;

        InitializeAsync().GetAwaiter().GetResult();
    }

    public async Task<Room> InsertAsync(Room room)
    {
        return (await _roomsContainer.CreateItemAsync(room)).Resource;
    }

    public async Task<Room> ReplaceAsync(Room room)
    {
        return (await _roomsContainer.ReplaceItemAsync(room, room.Id)).Resource;
    }

    public async Task<Room> UpsertAsync(Room room)
    {
        return (await _roomsContainer.UpsertItemAsync(room)).Resource;
    }

    public Task UpsertManyAsync(IEnumerable<Room> rooms)
    {
        var replacements = rooms.Select(room => _roomsContainer.ReplaceItemAsync(room, room.Id));

        return Task.WhenAll(replacements);
    }

    public async Task<Room> ReadByIdAsync(string roomId)
    {
        var query =
            new QueryDefinition(@"SELECT * FROM Room r WHERE r.id = @roomId").WithParameter(
                "@roomId",
                roomId);
        return (await ReadFromFeedAsync<Room>(query)).FirstOrDefault();
    }

    public async Task<Room> ReadByNameAsync(string roomName)
    {
        var query =
            new QueryDefinition(@"SELECT * FROM Room r WHERE r.name = @roomName")
                .WithParameter(
                    "@roomName",
                    roomName);

        return (await ReadFromFeedAsync<Room>(query)).FirstOrDefault();
    }

    public Task<List<Room>> ReadAllRoomsWithUser(string userName)
    {
        var query =
            new QueryDefinition(
                    @"SELECT * FROM Room r
                    WHERE ARRAY_CONTAINS(r.wasOnceConnectedUserNames, @userName)")
                .WithParameter(
                    "@userName",
                    userName);

        return ReadFromFeedAsync<Room>(query);
    }

    public Task<List<Room>> ReadAllAsync()
    {
        var query = new QueryDefinition(@"SELECT * FROM Room");

        return ReadFromFeedAsync<Room>(query);
    }

    private async Task InitializeAsync()
    {
        var cosmosClient = new CosmosClient(_cosmosSettings.Host, _cosmosSettings.Secret);

        var roomsContainerOptions = new ContainerProperties
        {
            Id = nameof(Room),
            PartitionKeyPath = "/type",
        };

        var database = (await cosmosClient.CreateDatabaseIfNotExistsAsync(_cosmosSettings.Database))
            .Database;

        _roomsContainer = (await database.CreateContainerIfNotExistsAsync(roomsContainerOptions))
            .Container;
    }

    private async Task<List<T>> ReadFromFeedAsync<T>(QueryDefinition query)
    {
        using var feed = _roomsContainer.GetItemQueryIterator<T>(query);
        var entities = new List<T>();

        while (feed.HasMoreResults)
        {
            var response = await feed.ReadNextAsync();
            entities.AddRange(response);
        }

        return entities;
    }
}
