using SOL.Dashboard.Dashboard.Features.Employee.Queries.GetAll;
using SOL.Domain.Entities.Security;
using Template.Domain.Specifications;

namespace Template.Dashboard.Dashboard.Features.Employee.Queries.GetAll;

public class GetAllEmployeesSpecification : Specification<AppUser>
{
    public GetAllEmployeesSpecification(GetAllEmployeesQuery.Request request)
    {
        ApplyFilters(e =>
            // Search Filter
            ((request.Search == null || request.Search == "") ||
             (e.FirstName.Contains(request.Search) ||
              e.LastName.Contains(request.Search) ||
              (e.Email ?? "").Contains(request.Search) ||
              (e.PhoneNumber ?? "").Contains(request.Search)
             ))
            &&
            // Active Status Filter
            (!request.Status.HasValue || (request.Status.Value == e.Status)));
    }
}

