using Microsoft.Azure.Cosmos;
using TeamCubing.Domain.Models;

namespace TeamCubing.DAL.Interfaces;

public interface IRoomRepository
{
    Task<ItemResponse<Room>> InsertAsync(Room room);

    Task<ItemResponse<Room>> ReplaceAsync(Room room);

    Task<ItemResponse<Room>> UpsertAsync(Room room);

    Task UpsertManyAsync(IEnumerable<Room> rooms);

    Task<Room> ReadByIdAsync(string roomId);

    Task<Room> ReadByNameAsync(string roomName);

    Task<List<Room>> ReadAllRoomsWithUser(string userName);

    Task<List<Room>> ReadAllAsync();
}
