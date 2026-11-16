namespace Praktikum.Data.Repositories;

/// <summary>Споделя един DbContext между всички хранилища в рамките на една заявка.</summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Dictionary<Type, object> _repositories = new();

    public UnitOfWork(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public IRepository<TEntity> Repository<TEntity>()
        where TEntity : class
    {
        if (!_repositories.TryGetValue(typeof(TEntity), out var repository))
        {
            repository = new EfRepository<TEntity>(_dbContext);
            _repositories[typeof(TEntity)] = repository;
        }

        return (IRepository<TEntity>)repository;
    }

    public Task<int> SaveChangesAsync() => _dbContext.SaveChangesAsync();
}
