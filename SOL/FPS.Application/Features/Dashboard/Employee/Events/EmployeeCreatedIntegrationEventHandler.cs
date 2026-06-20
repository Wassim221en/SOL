using Microsoft.Extensions.Logging;

namespace Template.Dashboard.Dashboard.Features.Employee.Events;

/// <summary>
/// Handler for EmployeeCreatedIntegrationEvent
/// This is an example handler that sends a welcome email when an employee is created
/// </summary>
public class EmployeeCreatedIntegrationEventHandler : IIntegrationEventHandler<EmployeeCreatedIntegrationEvent>
{
    private readonly ILogger<EmployeeCreatedIntegrationEventHandler> _logger;
    private readonly IEmailService _emailService;

    public EmployeeCreatedIntegrationEventHandler(
        ILogger<EmployeeCreatedIntegrationEventHandler> logger,
        IEmailService emailService)
    {
        _logger = logger;
        _emailService = emailService;
    }

    public async System.Threading.Tasks.Task HandleAsync(EmployeeCreatedIntegrationEvent @event, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Handling EmployeeCreatedIntegrationEvent for employee {EmployeeId} - {Email}",
            @event.EmployeeId,
            @event.Email);

        try
        {
            // Send welcome email
            var employeeName = $"{@event.FirstName} {@event.LastName}";
            await _emailService.SendWelcomeEmailAsync(@event.Email, employeeName);

            _logger.LogInformation(
                "Successfully sent welcome email to {Email}",
                @event.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error sending welcome email to {Email}",
                @event.Email);
            
            // Don't throw - we don't want to fail the entire event processing
            // You might want to implement a retry mechanism or dead letter queue
        }
    }
}

