

using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;
using Template.Domain.Primitives.Entity.Interface;
using Template.Domain.Specifications;
using Template.Persistence.DbContext;

namespace Template.Infrastructe.Services;

public class Repository : IRepository
{
    private readonly TemplateDbContext _context;

    public Repository(TemplateDbContext context)
    {
        _context = context;
    }

    public IQueryable<T> QueryWithDeleted<T>() where T :class,IBaseEntity<Guid>
    {
        return _context.Set<T>().AsNoTracking();
    }

    public IQueryable<T> Query<T>() where T : class,IBaseEntity<Guid>
    {
        return _context.Set<T>().AsNoTracking().Where(e => !e.DateDeleted.HasValue);
    }

    public IQueryable<T> TrackingQuery<T>() where T : class,IBaseEntity<Guid>
    {
        return _context.Set<T>().Where(e => !e.DateDeleted.HasValue);
    }

    public async Task<T?> GetByIdAsync<T>(Guid id, CancellationToken cancellationToken = default) where T : class,IBaseEntity<Guid>
    {
        return await _context.Set<T>()
            .FirstOrDefaultAsync(e => e.Id == id && !e.DateDeleted.HasValue, cancellationToken);
    }

    public async Task AddAsync<T>(T entity) where T : class,IBaseEntity<Guid>
    {
        entity.DateCreated=DateTimeOffset.UtcNow;
        await _context.Set<T>().AddAsync(entity);
    }

    public void Update<T>(T entity) where T : class,IBaseEntity<Guid>
    {
        entity.DateUpdated=DateTimeOffset.UtcNow;
        _context.Set<T>().Update(entity);
    }

    public void UpdateRange<T>(IEnumerable<T> entities) where T : class,IBaseEntity<Guid>
    {
        _context.Set<T>().UpdateRange(entities);
    }

    public void Delete<T>(T entity) where T : class,IBaseEntity<Guid>
    {
        _context.Set<T>().Remove(entity);
    }

    public void DeleteRange<T>(IEnumerable<T> entities) where T : class,IBaseEntity<Guid>
    {
        _context.Set<T>().RemoveRange(entities);
    }

    public void SoftDelete<T>(T entity) where T : class,IBaseEntity<Guid>
    {
        entity.DateDeleted = DateTime.UtcNow;
        Update(entity);
    }

    public void SoftDeleteRange<T>(IEnumerable<T> entities) where T : class,IBaseEntity<Guid>
    {
        foreach (var entity in entities)
        {
            entity.DateDeleted = DateTime.UtcNow;
        }

        UpdateRange(entities);
    }

    public IEnumerable<EntityEntry> GetAllEntries()
    {
        return _context.ChangeTracker.Entries();
    }

    public virtual async  Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<T>> GetAsync<T>(ISpecification<T> specification) where T : class,IBaseEntity<Guid>
    {
        var query = SpecificationEvaluator.GetQuery(Query<T>(), specification);
        return await query.ToListAsync();
    }

    public async Task<TResult?> GetAsync<T, TResult>(ISpecification<T> specification, Expression<Func<T, TResult>> selector) where T : class,IBaseEntity<Guid>
    {
        var query = SpecificationEvaluator.GetQuery(Query<T>(), specification);
        return await query.Select(selector).FirstOrDefaultAsync();
    }
    public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken)
    {
        return _context.Database.BeginTransactionAsync(cancellationToken);
    }
}
