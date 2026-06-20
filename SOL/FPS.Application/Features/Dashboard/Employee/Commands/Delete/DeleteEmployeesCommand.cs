using MediatR;

namespace FPS.Dashboard.Dashboard.Features.Employee.Commands.Delete;

public class DeleteEmployeesCommand
{
    public class Request : IRequest<OperationResponse>
    {
        public Request(Guid? id, List<Guid> ids)
        {
            if(id is not null)
                EmployeeIds.Add(id.Value);
            EmployeeIds.AddRange(ids);
        }
        public List<Guid> EmployeeIds { get; set; } = new();
    }
}

