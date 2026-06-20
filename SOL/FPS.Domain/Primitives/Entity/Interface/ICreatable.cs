namespace Template.Domain.Primitives.Entity.Interface;

public interface ICreatable<TIndex>  where TIndex : struct
{
    public DateTimeOffset DateCreated { get; set; }
    public TIndex? CreatedBy { get; set; }
}