using System.Net;
using SOL.Dashboard.Dashboard.Features.Employee.Commands.Auth.Commands.Login;
using SOL.Domain.Entities.Security;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace SOL.Dashboard.Dashboard.Features.Employee.Commands.Auth.Commands.ResetPassword;

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand.Request, OperationResponse<LoginCommand.Response>>
{
    private readonly IAuthService<AppUser> _authService;
    private readonly UserManager<AppUser> _userManager;
    private readonly IEventBus _eventBus;
    private readonly IRepository _repository;
    private readonly IMediator _mediator;
    public ResetPasswordCommandHandler(
        IAuthService<AppUser> authService,
        UserManager<AppUser> userManager,
        IEventBus eventBus, IRepository repository, IMediator mediator)
    {
        _authService = authService;
        _userManager = userManager;
        _eventBus = eventBus;
        _repository = repository;
        _mediator = mediator;
    }

    public async Task<OperationResponse<LoginCommand.Response>> Handle(ResetPasswordCommand.Request request, CancellationToken cancellationToken)
    {
        
        // Validate passwords match
        if (request.NewPassword != request.ConfirmPassword)
            return new HttpMessage("كلمتا المرور غير متطابقتين!", HttpStatusCode.BadRequest);

        // Find user by email
        var employee = await _repository.TrackingQuery<AppUser>()
            .FirstOrDefaultAsync(e=>e.Email==request.Email||e.PhoneNumber==request.PhoneNumber, cancellationToken: cancellationToken);
        if (employee is null)
            return new HttpMessage("المستخدم غير موجود!", HttpStatusCode.NotFound);
        
        var hashedToken = _authService.HashToken(request.ResetToken);
        if (!employee.ValidateResetPasswordToken(hashedToken))
            return new HttpMessage("رمز إعادة التعيين غير صحيح أو منتهي الصلاحية!", HttpStatusCode.BadRequest);

        // Remove old password and set new one
        var removePasswordResult = await _userManager.RemovePasswordAsync(employee);
        if (!removePasswordResult.Succeeded)
            return new HttpMessage("فشل إعادة تعيين كلمة المرور!", HttpStatusCode.InternalServerError);

        var addPasswordResult = await _userManager.AddPasswordAsync(employee, request.NewPassword);
        if (!addPasswordResult.Succeeded)
        {
            var errors = string.Join(", ", addPasswordResult.Errors.Select(e => e.Description));
            return new HttpMessage($"فشل تعيين كلمة المرور الجديدة: {errors}", HttpStatusCode.BadRequest);
        }

        // Clear reset token
        employee.ClearResetPasswordToken();
        await _userManager.UpdateAsync(employee);

        // Publish integration event
        var integrationEvent = new PasswordResetSuccessIntegrationEvent(
            employee.FullName,
            employee.Email!);
        await _eventBus.PublishAsync(integrationEvent, cancellationToken);
        var response = await _mediator.Send(new LoginCommand.Request()
        {
            Email = employee.Email,
            PhoneNumber = employee.PhoneNumber,
            Password = request.NewPassword,
        }, cancellationToken);
        return response;
    }
}
