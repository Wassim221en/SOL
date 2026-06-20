
using Template.Domain.Primitives.Entity.Interface;

namespace Template.Domain.Primitives.Entity.Interface;

public interface IAggregateRoot<TIndex> : IBaseEntity<TIndex> where TIndex : struct, IEquatable<TIndex>
{
   // public IReadOnlyCollection<IDomainEvent> DomainEvents { get; }
    public void ClearDomainEvents();

  
}