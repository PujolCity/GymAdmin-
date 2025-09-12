using GymAdmin.Domain.Entities;
using GymAdmin.Domain.Interfaces.Repositories;
using GymAdmin.Domain.Interfaces.Services;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace GymAdmin.Infrastructure.Data.Repositories;

public class Repository<T> : IRepository<T> where T : EntityBase
{
    protected readonly GymAdminDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(GymAdminDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    // --- Métodos de Lectura (sin guardar) ---
    public async Task<T> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var entity = await _dbSet.FindAsync(id, ct);

        if (entity is IEncryptableEntity encryptable)
            encryptable.HandleDecryption(_context._cryptoService);

        return entity;
    }

    public async Task<IEnumerable<T>> GetAllAsync(CancellationToken ct = default)
    {
        var list = await _dbSet.ToListAsync(ct);

        foreach (var entity in list.OfType<IEncryptableEntity>())
            entity.HandleDecryption(_context._cryptoService);

        return list;
    }

    public async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
        => await _dbSet.AnyAsync(predicate, ct);

    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
    {
        var list = await _dbSet.Where(predicate).Where(e => !e.IsDeleted).ToListAsync(ct);

        foreach (var entity in list.OfType<IEncryptableEntity>())
            entity.HandleDecryption(_context._cryptoService);

        return list;
    }

    public async Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
    {
        var entity = await _dbSet.Where(predicate).Where(e => !e.IsDeleted).FirstOrDefaultAsync(ct);

        if (entity is IEncryptableEntity encryptable)
            encryptable.HandleDecryption(_context._cryptoService);

        return entity;
    }

    // --- Métodos de Escritura (no guardan, solo cambian estado) ---
    public async Task AddAsync(T entity, CancellationToken ct)
        => await _dbSet.AddAsync(entity, ct);

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

    public async Task SoftDeleteAsync(int id, CancellationToken ct = default)
    {
        var entity = await _dbSet.FindAsync(id, ct);
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

    public async Task RestoreAsync(int id, CancellationToken ct = default)
    {
        var entity = await _dbSet.IgnoreQueryFilters().FirstOrDefaultAsync(e => e.Id == id, ct);
        if (entity != null)
            Restore(entity);
    }
}
