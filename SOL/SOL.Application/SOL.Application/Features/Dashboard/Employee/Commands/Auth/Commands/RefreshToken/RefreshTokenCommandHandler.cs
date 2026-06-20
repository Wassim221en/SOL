using System.Net;
using SOL.Dashboard.Dashboard.Features.Employee.Commands.Auth.Commands.Login;
using SOL.Domain.Entities.Security;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace SOL.Dashboard.Dashboard.Features.Employee.Commands.Auth.Commands.RefreshToken;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand.Request, OperationResponse<LoginCommand.Response>>
{
    private readonly IAuthService<AppUser> _authService;
    private readonly UserManager<AppUser> _userManager;
    private readonly IRepository _repository;
    private readonly IHttpContextService _httpContextService;

    public RefreshTokenCommandHandler(IAuthService<AppUser> authService, UserManager<AppUser> userManager, IRepository repository, IHttpContextService httpContextService)
    {
        _authService = authService;
        _userManager = userManager;
        _repository = repository;
        _httpContextService = httpContextService;
    }

    public async Task<OperationResponse<LoginCommand.Response>> Handle(RefreshTokenCommand.Request request, CancellationToken cancellationToken)
    {
        var refreshToken = await _authService.GenerateRefreshToken(null,request.RefreshToken,cancellationToken);
        if (!refreshToken.Success)
            return refreshToken.ErrorMessage!;
        
        var employee = await _repository.TrackingQuery<AppUser>()
            .Include(e => e.RefreshTokens)
            .Include(e=>e.UserRoles)
            .Include(e=>e.UserClaims)
            .FirstOrDefaultAsync(e => e.Id == refreshToken.Data!.UserId, cancellationToken: cancellationToken);
        if (employee is null)
            return new HttpMessage("المستخدم غير موجود!", HttpStatusCode.NotFound);
        var token = await _authService.GenerateAccessToken(employee, refreshToken.Data.DeviceId);
        var roleIds = employee.UserRoles.Select(ur => ur.RoleId);
        var roles = await _repository.Query<Domain.Entities.Security.Role>()
            .Include(r=>r.RoleClaims)
            .Where(r => roleIds.Contains(r.Id)).ToListAsync(cancellationToken);
        return new LoginCommand.Response()
        {
            UserId = employee.Id,
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            Email = employee.Email ?? "",
            UserName = employee.UserName??"",
            RefreshToken = refreshToken.Data!.RefreshToken!,
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
