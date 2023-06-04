using TeamCubing.DAL.Data;
using TeamCubing.DAL.Interfaces;

namespace TeamCubing.DAL.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private readonly Dictionary<Type, object> _repositories = new();

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    public IRepository<T> GetRepository<T>() where T : class
    {
        if (_repositories.ContainsKey(typeof(T)))
        {
            return (IRepository<T>)_repositories[typeof(T)];
        }

        var repo = new Repository<T>(_context);

        _repositories.Add(typeof(T), repo);

        return repo;
    }

    public Task SaveAsync()
    {
        return _context.SaveChangesAsync();
    }
}
