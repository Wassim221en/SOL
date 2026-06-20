namespace Template.Domain.Primitives.Entity.Interface;

public interface IUpdatable<TIndex> where TIndex : struct
{
    public DateTimeOffset? DateUpdated { get; set; }

    public TIndex? UpdatedBy { get; set; }
}