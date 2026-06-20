using FPS.Domain.Entities.Security;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FPS.Dashboard.Dashboard.Features.Employee.Commands.ModifyMyProfile;

public class ModifyMyProfileCommandHandler : IRequestHandler<ModifyMyProfileCommand.Request, OperationResponse<ModifyMyProfileCommand.Response>>
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IRepository _repository;
    private readonly IHttpContextService _httpContextService;
    private readonly IFileService _fileService;
    private readonly IEmployeeNamesCache _employeeNamesCache;

    public ModifyMyProfileCommandHandler(
        UserManager<AppUser> userManager,
        IRepository repository,
        IHttpContextService httpContextService,
        IFileService fileService,
        IEmployeeNamesCache employeeNamesCache)
    {
        _userManager = userManager;
        _repository = repository;
        _httpContextService = httpContextService;
        _fileService = fileService;
        _employeeNamesCache = employeeNamesCache;
    }

    public async Task<OperationResponse<ModifyMyProfileCommand.Response>> Handle(ModifyMyProfileCommand.Request request, CancellationToken cancellationToken)
    {
        
        request.UserName = request.UserName.ToLower();
        if(request.Email is not null)
            request.Email = request.Email.ToLower();
        // Get current user ID from HTTP context
        if (!_httpContextService.TryGetCurrentUserId(out Guid userId))
            return EmployeeErrors.UnAuthenticated();

        // Get user from database
        var employee = await _repository.TrackingQuery<AppUser>()
            .FirstOrDefaultAsync(e => e.Id == userId, cancellationToken);

        if (employee is null)
            return EmployeeErrors.UnAuthenticated();

        if(employee.UserName!=null&&employee.UserName!.ToUpper()=="SUPERADMIN")
            return EmployeeErrors.CannotModifySuperAdmin();
        // Handle Image Upload/Delete
        var oldImageUrls = employee.ImageUrl;

        // If DeleteImage is true, delete the old image
        if (request.DeleteImage && (oldImageUrls.OriginalImageUrl is not null || oldImageUrls.ThumbnailUrl is not null))
        {
            if(oldImageUrls.OriginalImageUrl is not null)
                await _fileService.DeleteAsync(oldImageUrls.OriginalImageUrl);
            if(oldImageUrls.ThumbnailUrl is not null)
                await _fileService.DeleteAsync(oldImageUrls.ThumbnailUrl);
            employee.ClearImageUrls();
        }

        // If new image is provided, upload it
        if (request.Image is not null)
        {
            var (originalUrl, thumbnailUrl) = await _fileService.UploadWithThumbnailAsync(request.Image, "Employees/Images");
            // Delete old image if exists and DeleteImage was not already set
            if (!request.DeleteImage && (oldImageUrls.OriginalImageUrl is not null || oldImageUrls.ThumbnailUrl is not null))
            {
                if(oldImageUrls.OriginalImageUrl is not null)
                    await _fileService.DeleteAsync(oldImageUrls.OriginalImageUrl);
                if(oldImageUrls.ThumbnailUrl is not null)
                    await _fileService.DeleteAsync(oldImageUrls.ThumbnailUrl);
            }
            employee.UpdateImageUrls(originalUrl, thumbnailUrl);
        }

        // Update UserName if provided
        if (request.UserName != employee.UserName)
        {
            var existingUser = await _userManager.FindByNameAsync(request.UserName);
            if (existingUser != null && existingUser.Id != userId)
                return EmployeeErrors.UserNameAlreadyExists(request.UserName);
            employee.UserName = request.UserName;
        }

        // Update Email if provided
        if (request.Email != employee.Email)
        {
            // Check if email already exists
            if (request.Email is not null)
            {
                var existingUser = await _userManager.FindByEmailAsync(request.Email);

                if (existingUser != null && existingUser.Id != userId)
                    return EmployeeErrors.EmailAlreadyExists(request.Email);
            }
            employee.Email = request.Email;
            employee.EmailConfirmed = (request.Email==null);
        }
        
        var identityResult = await _userManager.UpdateAsync(employee);
        if (!identityResult.Succeeded)
        {
            var errors = string.Join(", ", identityResult.Errors.Select(e => e.Description));
            return EmployeeErrors.PasswordChangeFailed(errors);
        }

        // Ensure chat employee directory cache stays consistent after self profile updates (image).
        await _employeeNamesCache.InvalidateAsync(cancellationToken);
        return new ModifyMyProfileCommand.Response
        {
            Id = employee.Id,
            UserName = employee.UserName,
            Email = employee.Email,
            ImageUrl = employee.ImageUrl.OriginalImageUrl,
        };
    }
}
