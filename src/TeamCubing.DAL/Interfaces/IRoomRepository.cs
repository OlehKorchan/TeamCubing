using TeamCubing.Domain.Models;

namespace TeamCubing.DAL.Interfaces;

public interface IRoomRepository : ICrudRepository<Room>
{
    Task<Room> ReadByNameAsync(string roomName);

    Task<List<Room>> ReadAllRoomsWithUser(string userName);

    Task ReplaceManyAsync(IEnumerable<Room> items);

    Task<List<Room>> ReadAllAsync();

    Task<bool> RemoveAsync(string roomName);
}
