using Microsoft.EntityFrameworkCore;

namespace Praktikum.Data.Repositories;

/// <summary>Реализация на хранилището върху Entity Framework Core.</summary>
public class EfRepository<TEntity> : IRepository<TEntity>
    where TEntity : class
{
    private readonly DbSet<TEntity> _set;

    public EfRepository(ApplicationDbContext dbContext)
    {
        _set = dbContext.Set<TEntity>();
    }

    public IQueryable<TEntity> All() => _set;

    public IQueryable<TEntity> AllAsNoTracking() => _set.AsNoTracking();

    public async Task<TEntity?> GetByIdAsync(int id) => await _set.FindAsync(id);

    public async Task AddAsync(TEntity entity) => await _set.AddAsync(entity);

    public void Delete(TEntity entity) => _set.Remove(entity);
}
