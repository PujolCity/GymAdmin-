using GymAdmin.Domain.Entities;
using GymAdmin.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace GymAdmin.Infrastructure.Data.Repositories;

public class Repository<T> : IRepository<T> where T : EntityBase
{
    protected readonly DbSet<T> _dbSet;

    public Repository(GymAdminDbContext context)
    {
        _dbSet = context.Set<T>();
    }

    // --- Métodos de Lectura (sin guardar) ---
    public async Task<T> GetByIdAsync(int id)
        => await _dbSet.FindAsync(id);

    public async Task<IEnumerable<T>> GetAllAsync()
        => await _dbSet.Where(e => !e.IsDeleted).ToListAsync();

    public async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
        => await _dbSet.AnyAsync(predicate);

    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        => await _dbSet.Where(predicate).Where(e => !e.IsDeleted).ToListAsync();

    public async Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
        => await _dbSet.Where(predicate).Where(e => !e.IsDeleted).FirstOrDefaultAsync();

    // --- Métodos de Escritura (no guardan, solo cambian estado) ---
    public async Task AddAsync(T entity)
        => await _dbSet.AddAsync(entity);

    public void Update(T entity)
        => _dbSet.Update(entity);

    public void Remove(T entity)
        => _dbSet.Remove(entity);

    // --- Métodos para Query Composition ---
    public IQueryable<T> Query()
        => _dbSet.Where(e => !e.IsDeleted);

    public IQueryable<T> QueryWithIncludes(params Expression<Func<T, object>>[] includes)
    {
        var query = _dbSet.Where(e => !e.IsDeleted);
        foreach (var include in includes)
        {
            query = query.Include(include);
        }
        return query;
    }

    // --- Métodos de Soft Delete y Restore (solo marcan, no guardan) ---
    public void SoftDelete(T entity)
    {
        if (entity != null && !entity.IsDeleted)
        {
            entity.SoftDelete();
            Update(entity);
        }
    }

    public void SoftDelete(int id)
    {
        var entity = _dbSet.Find(id);
        if (entity != null)
            SoftDelete(entity);
    }

    public void Restore(T entity)
    {
        if (entity != null && entity.IsDeleted)
        {
            entity.Restore();
            Update(entity);
        }
    }

    public void Restore(int id)
    {
        var entity = _dbSet.IgnoreQueryFilters().FirstOrDefault(e => e.Id == id);
        if (entity != null)
            Restore(entity);
    }
}
