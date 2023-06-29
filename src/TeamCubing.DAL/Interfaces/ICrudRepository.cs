namespace TeamCubing.DAL.Interfaces;

public interface ICrudRepository<T>
{
    Task<T> InsertAsync(T item);

    Task<T> ReplaceAsync(T item);

    Task<T> UpsertAsync(T item);

    Task<T> ReadByIdAsync(string id);
}
