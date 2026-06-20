using Template.Domain.Specifications;

namespace SOL.Dashboard.Dashboard.Features.Role.Queries.GetAll;

public class GetAllRolesSpecification : Specification<SOL.Domain.Entities.Security.Role>
{
    public GetAllRolesSpecification(GetAllRolesQuery.Request request)
    {
    }
}