using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TeamCubing.DAL.Interfaces;
using TeamCubing.Domain.Models;
using TeamCubing.Domain.Settings;

namespace TeamCubing.DAL.Repositories;

public class RoomRepository : CosmosBaseRepository<Room>, IRoomRepository
{
    public RoomRepository(IOptions<Settings> settings, ILogger<RoomRepository> logger) :
        base(settings.Value.CosmosSettings, logger)
    {
    }

    public async Task<Room> InsertAsync(Room room)
    {
        return (await Container.CreateItemAsync(room))?.Resource;
    }

    public async Task<Room> ReplaceAsync(Room room)
    {
        return (await Container.ReplaceItemAsync(room, room.Id))?.Resource;
    }

    public async Task<Room> UpsertAsync(Room room)
    {
        return (await Container.UpsertItemAsync(room))?.Resource;
    }

    public Task ReplaceManyAsync(IEnumerable<Room> rooms)
    {
        var replacements = rooms.Select(room => Container.ReplaceItemAsync(room, room.Id));

        return Task.WhenAll(replacements);
    }

    public async Task<Room> ReadByIdAsync(string roomId)
    {
        var query =
            new QueryDefinition(@"SELECT * FROM Room r WHERE r.id = @roomId").WithParameter(
                "@roomId",
                roomId);
        return (await ReadFromFeedAsync(query))?.FirstOrDefault();
    }

    public async Task<Room> ReadByNameAsync(string roomName)
    {
        var query =
            new QueryDefinition(@"SELECT * FROM Room r WHERE r.name = @roomName")
                .WithParameter(
                    "@roomName",
                    roomName);

        return (await ReadFromFeedAsync(query))?.FirstOrDefault();
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

        return ReadFromFeedAsync(query);
    }

    public Task<List<Room>> ReadAllAsync()
    {
        var query = new QueryDefinition(@"SELECT * FROM Room");

        return ReadFromFeedAsync(query);
    }
}
