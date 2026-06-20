namespace SOL.Application.Common.Interfaces;

public interface IEmployeeNamesCache
{
    Task InvalidateAsync(CancellationToken cancellationToken = default);
}
