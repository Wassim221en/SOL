using System.Linq.Expressions;
using Template.Domain.Primitives.Entity.Interface;

namespace Template.Domain.Specifications;

public class Specification<T>:ISpecification<T> where T:IBaseEntity
{
    public Expression<Func<T, bool>> Criteria { get; protected set; } = x => true;
    public List<Expression<Func<T, object>>> Includes { get; protected set; } = new();
    public Expression<Func<T, object>>? OrderBy { get; protected set; }
    public Expression<Func<T, object>>? OrderByDescending { get; protected set; }
    public int? Take { get; protected set; }
    public int? Skip { get; protected set; }
    public bool IsPagingEnabled { get; protected set; } = false;
    protected void ApplyFilters(Expression<Func<T, bool>> criteria)
        => Criteria = criteria;
    
    protected void AddInclude(Expression<Func<T, object>> includeExpression)
        => Includes.Add(includeExpression);

    protected void AddOrderBy(Expression<Func<T, object>> orderByExpression)
        => OrderBy = orderByExpression;

    protected void AddOrderByDescending(Expression<Func<T, object>> orderByDescExpression)
        => OrderByDescending = orderByDescExpression;

    protected void ApplyPaging(int skip, int take)
    {
        Skip = skip;
        Take = take;
        IsPagingEnabled = true;
    }
}