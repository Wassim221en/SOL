using System.Net;
using FPS.Dashboard.Dashboard.Features.Employee.Commands.Auth.Commands.EmailConfirmed;
using FPS.Domain.Entities.Security;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FPS.Dashboard.Dashboard.Features.Employee.Commands.Auth.Commands.EmailVerification;

public class EmailConfirmedCommandHandler:IRequestHandler<EmailConfirmedCommand.Request,OperationResponse>
{
    private readonly IRepository _repository;
    private readonly IHttpContextService _httpContextService;
    private readonly IAuthService<AppUser> _authService;
    private readonly IEventBus _eventBus;
    public EmailConfirmedCommandHandler(IRepository repository, IHttpContextService httpContextService, IAuthService<AppUser> authService, IEventBus eventBus)
    {
        _repository = repository;
        _httpContextService = httpContextService;
        _authService = authService;
        _eventBus = eventBus;
    }

    public async Task<OperationResponse> Handle(EmailConfirmedCommand.Request request, CancellationToken cancellationToken)
    {
        var employee=await _repository.TrackingQuery<AppUser>()
            .FirstOrDefaultAsync(e=>e.Email==request.Email,cancellationToken);
        if (employee is null)
            return new HttpMessage("Employee not found.", HttpStatusCode.NotFound);
        var hashCode = _authService.HashToken(request.Code);
        if (employee.ResetPasswordToken != hashCode)
            return new HttpMessage("Invalid or expired confirmation code.", HttpStatusCode.BadRequest);
        employee.EmailConfirmed = true;
        employee.ClearResetPasswordToken();
        await _repository.SaveChangesAsync(cancellationToken);
        return OperationResponse.Ok();
    }
}
