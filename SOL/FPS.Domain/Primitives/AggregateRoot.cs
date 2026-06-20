using Template.Domain.Primitives.Entity.Base;
using Template.Domain.Primitives.Entity.Interface;

namespace Template.Domain.Primitives;

public abstract class AggregateRoot<TIndex> : BaseEntity<TIndex>, IAggregateRoot<TIndex> where TIndex : struct, IEquatable<TIndex>
{
    //private readonly List<IDomainEvent> _domainEvents = new();

    //public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    //public void ClearDomainEvents() => _domainEvents.Clear();

    //protected void AddDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
    public void ClearDomainEvents()
    {
        throw new NotImplementedException();
    }
}