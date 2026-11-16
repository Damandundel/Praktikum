namespace Praktikum.Data.Repositories;

/// <summary>
/// Единица работа: група промени в различни хранилища, записвани заедно в една транзакция.
/// Пример: при одобряване на кандидатура се сменя статусът И се създава известие;
/// двете стават с един SaveChangesAsync - или успяват и двете, или никоя.
/// </summary>
public interface IUnitOfWork
{
    IRepository<TEntity> Repository<TEntity>()
        where TEntity : class;

    Task<int> SaveChangesAsync();
}
