using FPS.Domain.Entities.Security;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FPS.Dashboard.Dashboard.Features.Employee.Commands.Delete;

public class DeleteEmployeesHandler : IRequestHandler<DeleteEmployeesCommand.Request, OperationResponse>
{
    private readonly IRepository _repository;
    private readonly IEmployeeNamesCache _employeeNamesCache;
    private readonly ITokenCacheService _tokenCacheService;

    public DeleteEmployeesHandler(
        IRepository repository,
        IEmployeeNamesCache employeeNamesCache,
        ITokenCacheService tokenCacheService)
    {
        _repository = repository;
        _employeeNamesCache = employeeNamesCache;
        _tokenCacheService = tokenCacheService;
    }

    public async Task<OperationResponse> Handle(DeleteEmployeesCommand.Request request, CancellationToken cancellationToken)
    {
        //if (!request.EmployeeIds.Any())
            //return EmployeeErrors.NoEmployeeIdsProvided();

        // Get employees to delete
        var employees = await _repository.TrackingQuery<AppUser>()
            .Where(e => request.EmployeeIds.Contains(e.Id) && (e.UserName != null && e.UserName.ToUpper() != "SUPERADMIN"))
            .ToListAsync(cancellationToken);

        //if (!employees.Any())
            //return EmployeeErrors.NoEmployeesFound();

        // Soft delete employees
        _repository.SoftDeleteRange(employees);
        await _repository.SaveChangesAsync(cancellationToken);

        await Task.WhenAll(employees.Select(employee =>
            _tokenCacheService.InvalidateAllUserTokensAsync(employee.Id.ToString())));

        // Ensure chat employee directory cache stays consistent after deletes.
        await _employeeNamesCache.InvalidateAsync(cancellationToken);

        return OperationResponse.Ok();
    }
}
