using System.Net;
using SOL.Dashboard.Dashboard.Features.Employee.Commands.Auth.Commands.Login;
using SOL.Domain.Entities.Security;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Template.Domain.Primitives.Entity.Identity;

namespace Template.Dashboard.Dashboard.Features.Employee.Commands.Auth.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand.Request,OperationResponse<LoginCommand.Response>>
{
    private readonly IAuthService<AppUser> _authService;
    private readonly UserManager<AppUser> _userManager;
    private readonly IRepository _repository;
    private readonly IEventBus _eventBus;
    public LoginCommandHandler(IAuthService<AppUser> authService, UserManager<AppUser> userManager, IRepository repository, IEventBus eventBus)
    {
        _authService = authService;
        _userManager = userManager;
        _repository = repository;
        _eventBus = eventBus;
    }

    public async Task<OperationResponse<LoginCommand.Response>> Handle(LoginCommand.Request request, CancellationToken cancellationToken)
    {
        if(request.UserName is not null)
            request.UserName = request.UserName.ToLower();
        if(request.Email is not null)
            request.Email = request.Email.ToLower();
        var hasEmail = !string.IsNullOrWhiteSpace(request.Email);
        var hasPhoneNumber = !string.IsNullOrWhiteSpace(request.PhoneNumber);
        var hasUserName = !string.IsNullOrWhiteSpace(request.UserName);

        var identifiersCount = (hasEmail ? 1 : 0) + (hasPhoneNumber ? 1 : 0) + (hasUserName ? 1 : 0);
        if (identifiersCount == 0)
            return EmployeeErrors.LoginIdentifierIsRequired();
        if (identifiersCount > 1)
            return EmployeeErrors.OnlyOneLoginIdentifierAllowed();

        var invalidCredentialsError = hasEmail
            ? EmployeeErrors.InvalidPasswordOrEmail
            : hasPhoneNumber
                ? EmployeeErrors.InvalidPasswordOrPhoneNumber
                : EmployeeErrors.InvalidPasswordOrUserName;

        var usersQuery = _userManager.Users
            .Where(u => !u.DateDeleted.HasValue)
            .Include(e => e.RefreshTokens)
            .Include(e => e.UserRoles)
            .Include(e => e.UserClaims)
            .AsQueryable();

        var employee = hasEmail
            ? await usersQuery.FirstOrDefaultAsync(
                e => e.Email != null && e.Email.ToLower() == request.Email,
                cancellationToken: cancellationToken)
            : hasPhoneNumber
                ? await usersQuery.FirstOrDefaultAsync(
                    e => e.PhoneNumber != null && e.PhoneNumber == request.PhoneNumber,
                    cancellationToken: cancellationToken)
                : await usersQuery.FirstOrDefaultAsync(
                    e => e.UserName != null && e.UserName.ToLower() == request.UserName,
                    cancellationToken: cancellationToken);

        if (employee is null)
            return invalidCredentialsError;
        if (!await _userManager.CheckPasswordAsync(employee, request.Password))
            return invalidCredentialsError;
        /*if (employee.EmailConfirmed is false && employee.Email is not null)
        {
            var resetToken =  _authService.GenerateResetToken();
            employee.SetResetPasswordToken(_authService.HashToken(resetToken),DateTime.UtcNow.AddHours(1));
            _repository.Update(employee);
            await _repository.SaveChangesAsync(cancellationToken);
            var emailVerificationEvent=new EmailVerificationIntegrationEvent(employee.Email,resetToken,$"{employee.FirstName} {employee.LastName}");
            await _eventBus.PublishAsync(emailVerificationEvent, cancellationToken);
            return new HttpMessage("البريد الإلكتروني غير مؤكد", HttpStatusCode.BadRequest);
        }*/
        var token = await _authService.GenerateAccessToken(employee);
        var refreshToken = await _authService.GenerateRefreshToken(employee.Id,null, cancellationToken);
        if (refreshToken.Success is false)
            return refreshToken.ErrorMessage!;
        if (request.DeviceToken is not null)
        {
            employee.DeviceTokens.Add(request.DeviceToken);
            await _userManager.UpdateAsync(employee);
        }
        var roleIds = employee.UserRoles.Select(ur => ur.RoleId);
        var roles = await _repository.Query<SOL.Domain.Entities.Security.Role>()
            .Include(r=>r.RoleClaims)
            .Where(r => roleIds.Contains(r.Id)).ToListAsync(cancellationToken);
        return new LoginCommand.Response()
        {
            UserId = employee.Id,
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            Email = employee.Email ?? "",
            ImageUrl = employee.ImageUrl.OriginalImageUrl,
            ThumbnailUrl = employee.ImageUrl.ThumbnailUrl,
            UserName = employee.UserName ?? "",
            RefreshToken = refreshToken.Data!.RefreshToken,
            Token = token,
            Roles = roles.Select(r => new LoginCommand.Response.RoleRes
            {
                RoleId = r.Id,
                RoleName = r.Name ?? "",
                PermissionsPages =  r.RoleClaims
                    .Select(c => c.ClaimValue)
                    .Where(v => v.Contains('.'))
                    .Select(v => new
                    {
                        Page = v.Split('.')[0],
                        Permission = v.Split('.')[1]
                    })
                    .GroupBy(x => x.Page)
                    .Select(g => new PermissionsPage
                    {
                        Page = g.Key,
                        Permissions = g.Select(x => x.Permission).ToList()
                    })
                    .ToList()
            }).ToList()
        };
    }
}
