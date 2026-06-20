namespace Template.Domain.Primitives.Entity.Interface;

public interface IDeletable<TIndex>  where TIndex : struct
{
    public TIndex? DeletedBy { get; set; }
    public DateTimeOffset? DateDeleted { get; set; }
}