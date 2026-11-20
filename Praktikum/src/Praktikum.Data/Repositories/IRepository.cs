namespace Praktikum.Data.Repositories;

/// <summary>
/// Общ достъп до данните на една entity. Скрива EF Core от услугите,
/// така че бизнес логиката да се тества лесно и да не зависи от начина на съхранение.
/// </summary>
public interface IRepository<TEntity>
    where TEntity : class
{
    /// <summary>Заявка със следене на промените (за редактиране).</summary>
    IQueryable<TEntity> All();

    /// <summary>Заявка без следене на промените (по-бърза, за четене).</summary>
    IQueryable<TEntity> AllAsNoTracking();

    Task<TEntity?> GetByIdAsync(int id);

    Task AddAsync(TEntity entity);

    void Delete(TEntity entity);
}
