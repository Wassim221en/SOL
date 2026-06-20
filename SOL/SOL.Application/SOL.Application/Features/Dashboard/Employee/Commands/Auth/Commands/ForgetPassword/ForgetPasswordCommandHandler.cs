using SOL.Domain.Entities.Security;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace SOL.Dashboard.Dashboard.Features.Employee.Commands.Auth.Commands.ForgetPassword;

public class ForgetPasswordCommandHandler : IRequestHandler<ForgetPasswordCommand.Request,OperationResponse>
{
    private readonly IAuthService<AppUser> _authService;
    private readonly UserManager<AppUser> _userManager;
    private readonly IEventBus _eventBus;
    private readonly IWhatsAppService _whatsAppService;
    private readonly IRepository _repository;

    public ForgetPasswordCommandHandler(IAuthService<AppUser> authService, UserManager<AppUser> userManager, IEventBus eventBus, IWhatsAppService whatsAppService, IRepository repository)
    {
        _authService = authService;
        _userManager = userManager;
        _eventBus = eventBus;
        _whatsAppService = whatsAppService;
        _repository = repository;
    }

    public async Task<OperationResponse> Handle(ForgetPasswordCommand.Request request, CancellationToken cancellationToken)
    {
        var employee=await _repository.TrackingQuery<AppUser>()
            .FirstOrDefaultAsync(e=>e.Email==request.Email|| e.PhoneNumber==request.PhoneNumber, cancellationToken: cancellationToken);
        if (employee is null)
            return EmployeeErrors.NotFound();
        var resetToken =  _authService.GenerateResetToken();
        employee.SetResetPasswordToken( _authService.HashToken(resetToken),DateTime.UtcNow.AddHours(1));
        await _userManager.UpdateAsync(employee);
        var forgetPasswordIntegrationEvent=new ForgetPasswordIntegrationEvent(employee.FullName,resetToken,employee.Email!,employee.PhoneNumber);
        await _eventBus.PublishAsync(forgetPasswordIntegrationEvent, cancellationToken);
        return OperationResponse.Ok();
    }
}

