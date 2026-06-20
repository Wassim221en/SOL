using Template.Domain.Specifications;

namespace FPS.Dashboard.Dashboard.Features.Role.Queries.GetAll;

public class GetAllRolesSpecification : Specification<FPS.Domain.Entities.Security.Role>
{
    public GetAllRolesSpecification(GetAllRolesQuery.Request request)
    {
    }
}