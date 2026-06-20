using System.Net;
using SOL.Domain.Entities.Security;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace SOL.Dashboard.Dashboard.Features.Employee.Commands.Auth.Commands.CodeVerification;

public class CodeVerificationHandler:IRequestHandler<CodeVerificationCommand.Request,OperationResponse>
{
    private readonly IAuthService<AppUser> _authService;
    private readonly UserManager<AppUser> _userManager;
    private readonly IRepository _repository;

    public CodeVerificationHandler(IAuthService<AppUser> authService, UserManager<AppUser> userManager, IRepository repository)
    {
        _authService = authService;
        _userManager = userManager;
        _repository = repository;
    }

    public async Task<OperationResponse> Handle(CodeVerificationCommand.Request request, CancellationToken cancellationToken)
    {
        var employee = await _repository.Query<AppUser>()
            .FirstOrDefaultAsync(e=>e.Email==request.Email||e.PhoneNumber==request.PhoneNumber, cancellationToken: cancellationToken);
        if (employee is null)
            return new HttpMessage("المستخدم غير موجود!", HttpStatusCode.NotFound);

        // Hash the provided token and validate it
        var hashedToken = _authService.HashToken(request.ResetToken);
        if (!employee.ValidateResetPasswordToken(hashedToken))
            return new HttpMessage("رمز إعادة التعيين غير صحيح أو منتهي الصلاحية!", HttpStatusCode.BadRequest);
        return OperationResponse.Ok();
    }
}
