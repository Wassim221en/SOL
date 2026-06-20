using Template.Domain.Events;

namespace SOL.Domain.Events.Employee;

public class EmployeeCreatedIntegrationEvent : IntegrationEvent
{
    public Guid EmployeeId { get; }
    public string FirstName { get; }
    public string LastName { get; }
    public string Email { get; }
    public string? PhoneNumber { get; }

    public EmployeeCreatedIntegrationEvent(Guid employeeId, string firstName, string lastName, string email, string? phoneNumber)
    {
        EmployeeId = employeeId;
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        PhoneNumber = phoneNumber;
    }
}
