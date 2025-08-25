using GymAdmin.Domain.Entities;
using System.Linq.Expressions;

namespace GymAdmin.Domain.Interfaces.Repositories;

public interface IRepository<T> where T : EntityBase
{
    Task<T> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task AddAsync(T entity);
    void Update(T entity);
    void Remove(T entity);
    void SoftDelete(T entity);
    void SoftDelete(int id);
    void Restore(T entity);
    void Restore(int id);

    // Métodos de consulta
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);

    // Métodos con Includes
    IQueryable<T> Query(); // Para composición de queries
    IQueryable<T> QueryWithIncludes(params Expression<Func<T, object>>[] includes);
}
