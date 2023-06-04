using System.Linq.Expressions;

namespace TeamCubing.DAL.Interfaces;

public interface IRepository<T> where T : class
{
    IQueryable<T> AsQueryable();
    Task<T> GetOneAsync(Expression<Func<T, bool>> predicate);
    Task<T> GetOneTrackingAsync(Expression<Func<T, bool>> predicate);
    Task<List<T>> GetManyAsync(Expression<Func<T, bool>> predicate);
    Task<T> CreateOneAsync(T item);
    T UpdateOne(T item);
    Task DeleteOneAsync(Expression<Func<T, bool>> predicate);
    void DeleteMany(Expression<Func<T, bool>> predicate);
}
