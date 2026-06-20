using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Template.Domain.Primitives.Entity.Interface;
using Template.Domain.Specifications;

public static class SpecificationEvaluator
{
    public static IQueryable<T> GetQuery<T>(IQueryable<T> inputQuery, ISpecification<T> specification)
        where T :class, IBaseEntity<Guid>
    {
        var query = inputQuery;

        query = query.Where(specification.Criteria);

        if (specification.OrderBy is not null)
            query = query.OrderBy(specification.OrderBy);

        if (specification.OrderByDescending is not null)
            query = query.OrderByDescending(specification.OrderByDescending);

        if (specification.IsPagingEnabled)
        {
            if (specification.Skip.HasValue)
                query = query.Skip(specification.Skip.Value);
            if (specification.Take.HasValue)
                query = query.Take(specification.Take.Value);
        }

        foreach (var include in specification.Includes)
            query = query.Include(include);

        return query;
    }
}