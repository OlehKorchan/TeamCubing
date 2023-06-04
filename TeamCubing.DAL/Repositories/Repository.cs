using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using TeamCubing.DAL.Interfaces;

namespace TeamCubing.DAL.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    private readonly DbSet<T> _dbSet;

    public Repository(DbContext context)
    {
        _dbSet = context.Set<T>();
    }

    public IQueryable<T> AsQueryable()
    {
        return _dbSet.AsQueryable();
    }

    public Task<T> GetOneAsync(Expression<Func<T, bool>> predicate)
    {
        return _dbSet.AsNoTracking().FirstOrDefaultAsync(predicate);
    }

    public Task<T> GetOneTrackingAsync(Expression<Func<T, bool>> predicate)
    {
        return _dbSet.FirstOrDefaultAsync(predicate);
    }

    public Task<List<T>> GetManyAsync(Expression<Func<T, bool>> predicate)
    {
        return _dbSet.AsNoTracking().Where(predicate).ToListAsync();
    }

    public async Task<T> CreateOneAsync(T item)
    {
        return (await _dbSet.AddAsync(item)).Entity;
    }

    public T UpdateOne(T item)
    {
        return _dbSet.Update(item).Entity;
    }

    public async Task DeleteOneAsync(Expression<Func<T, bool>> predicate)
    {
        var entity = await _dbSet.FirstAsync(predicate);

        _dbSet.Remove(entity);
    }

    public void DeleteMany(Expression<Func<T, bool>> predicate)
    {
        _dbSet.RemoveRange(_dbSet.Where(predicate));
    }
}
