using GymAdmin.Domain.Entities;
using System.Linq.Expressions;

namespace GymAdmin.Domain.Interfaces.Repositories;

public interface IRepository<T> where T : EntityBase
{
    Task<T> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<T>> GetAllAsync( CancellationToken ct = default);
    Task AddAsync(T entity, CancellationToken ct = default);
    void Update(T entity);
    void Remove(T entity);
    void SoftDelete(T entity);
    Task SoftDeleteAsync(int id, CancellationToken ct = default);
    void Restore(T entity);
    Task RestoreAsync(int id, CancellationToken ct = default);

    // Métodos de consulta
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);
    Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);

    // Métodos con Includes
    IQueryable<T> Query(); // Para composición de queries
    IQueryable<T> QueryWithIncludes(params Expression<Func<T, object>>[] includes);
}
