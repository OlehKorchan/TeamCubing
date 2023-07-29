using TeamCubing.Domain.Models;

namespace TeamCubing.DAL.Interfaces;

public interface IRoomRepository : ICrudRepository<Room>
{
    Task<Room> ReadByNameAsync(string roomName);

    Task<List<Room>> ReadAllRoomsWithUser(string userName);

    Task ReplaceManyAsync(IEnumerable<Room> items);

    Task<List<Room>> ReadAllAsync();

    Task ReplaceScrambleCachePatch(string roomName, List<string> newScrambles);

    Task InsertUserResultPatch(string roomName, int solveIndex, SolveResult newResult);

    Task InsertNewSolvePatch(string roomName, Solve solve);

    Task ReplaceSolvePatch(string roomName, int solveIndex, Solve solve);

    Task RemoveSolvePatch(string roomName, int solveIndex);

    Task<bool> RemoveAsync(string roomName);
}
