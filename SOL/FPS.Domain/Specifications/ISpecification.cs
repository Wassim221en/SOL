using System.Linq.Expressions;
using Template.Domain.Primitives.Entity.Interface;

namespace Template.Domain.Specifications;

public interface ISpecification<T> where T:IBaseEntity
{
    Expression<Func<T, bool>> Criteria { get; }

    List<Expression<Func<T, object>>> Includes { get; }

    Expression<Func<T, object>>? OrderBy { get; }

    Expression<Func<T, object>>? OrderByDescending { get; }

    int? Take { get; }

    int? Skip { get; }

    bool IsPagingEnabled { get; }
}
