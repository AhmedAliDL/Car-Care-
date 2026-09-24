using System.Linq.Expressions;
using App.Application.Abstractions.Persistence;
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure.Persistence.Repositories;

public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    private readonly CarCareDbContext _db;

    public GenericRepository(CarCareDbContext db)
    {
        _db = db;
    }

    public IQueryable<T> Query() => _db.Set<T>();

    public IQueryable<T> QueryNoTracking() => _db.Set<T>().AsNoTracking();

    public async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _db.Set<T>().FindAsync([id], cancellationToken);

    public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        => await _db.Set<T>().AnyAsync(predicate, cancellationToken);

    public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
        => await _db.Set<T>().AddAsync(entity, cancellationToken);

    public void Update(T entity) => _db.Set<T>().Update(entity);

    public void Remove(T entity) => _db.Set<T>().Remove(entity);
}
