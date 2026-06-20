using SOL.Domain.Entities.Security;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace SOL.Dashboard.Dashboard.Features.Employee.Commands.ModifyMyPassword;

public class ModifyMyPasswordHandler:IRequestHandler<ModifyMyPasswordCommand.Request,OperationResponse>
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IHttpContextService _httpContextService;

    public ModifyMyPasswordHandler(UserManager<AppUser> userManager, IHttpContextService httpContextService)
    {
        _userManager = userManager;
        _httpContextService = httpContextService;
    }

    public async Task<OperationResponse> Handle(ModifyMyPasswordCommand.Request request, CancellationToken cancellationToken)
    {
        var res =  _httpContextService.TryGetCurrentUserId(out Guid userId);
        if(res is false)
            return EmployeeErrors.UnAuthenticated();
        var employee=await _userManager.FindByIdAsync(userId.ToString());
        if (request.NewPassword != request.ConfirmNewPassword)
            return EmployeeErrors.NewPasswordsDoNotMatch();

        // Verify old password is correct
        var isOldPasswordCorrect = await _userManager.CheckPasswordAsync(employee, request.OldPassword);
        if (!isOldPasswordCorrect)
            return EmployeeErrors.OldPasswordIsIncorrect();

        // Change password
        var changePasswordResult = await _userManager.ChangePasswordAsync(employee, request.OldPassword, request.NewPassword);
        if (!changePasswordResult.Succeeded)
        {
            var errors = string.Join(", ", changePasswordResult.Errors.Select(e => e.Description));
            return EmployeeErrors.PasswordChangeFailed(errors);
        }
        return OperationResponse.Ok();
    }
}