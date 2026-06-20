


using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Storage;
using Template.Domain.Primitives.Entity.Interface;
using Template.Domain.Specifications;

public interface IRepository
{
    IQueryable<T> Query<T>() where T:class,IBaseEntity<Guid>;
    IQueryable<T> TrackingQuery<T>() where T:class,IBaseEntity<Guid>;
    IQueryable<T> QueryWithDeleted<T>() where T : class,IBaseEntity<Guid>;
    Task<T?> GetByIdAsync<T>(Guid id, CancellationToken cancellationToken = default) where T:class,IBaseEntity<Guid>;
    Task AddAsync<T>(T entity)where T:class,IBaseEntity<Guid>;
    void Update<T>(T entity)where T:class,IBaseEntity<Guid>;
    void UpdateRange<T>(IEnumerable<T> entities)where T:class,IBaseEntity<Guid>;
    void Delete<T>(T entity)where T:class,IBaseEntity<Guid>;
    void DeleteRange<T>(IEnumerable<T> entities)where T:class,IBaseEntity<Guid>;
    void SoftDelete<T>(T entity) where T:class,IBaseEntity<Guid>;
    void SoftDeleteRange<T>(IEnumerable<T> entities)where T:class,IBaseEntity<Guid>;
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken);
    Task<IEnumerable<T>>GetAsync<T>(ISpecification<T> specification)where T:class,IBaseEntity<Guid>;

    Task<TResult?> GetAsync<T, TResult>(ISpecification<T> specification, Expression<Func<T, TResult>> selector)
        where T : class,IBaseEntity<Guid>;
}