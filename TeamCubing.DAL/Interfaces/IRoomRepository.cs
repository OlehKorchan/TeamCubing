using TeamCubing.Domain.Models;

namespace TeamCubing.DAL.Interfaces;

public interface IRoomRepository
{
    Task<Room> InsertAsync(Room room);

    Task<Room> ReplaceAsync(Room room);

    Task<Room> UpsertAsync(Room room);

    Task ReplaceManyAsync(IEnumerable<Room> rooms);

    Task<Room> ReadByIdAsync(string roomId);

    Task<Room> ReadByNameAsync(string roomName);

    Task<List<Room>> ReadAllRoomsWithUser(string userName);

    Task<List<Room>> ReadAllAsync();
}
