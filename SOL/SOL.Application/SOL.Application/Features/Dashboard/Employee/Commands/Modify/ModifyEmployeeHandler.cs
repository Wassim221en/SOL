using System.Net;
using SOL.Dashboard.Dashboard.Features.Employee.Queries.GetById;
using SOL.Domain.Entities.Security;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace SOL.Dashboard.Dashboard.Features.Employee.Commands.Modify;

public class ModifyEmployeeHandler : IRequestHandler<ModifyEmployeeCommand.Request, OperationResponse<GetEmployeeByIdQuery.Response>>
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IRepository _repository;
    private readonly IFileService _fileService;
    private readonly IEmployeeNamesCache _employeeNamesCache;

    public ModifyEmployeeHandler(
        UserManager<AppUser> userManager,
        IRepository repository,
        IFileService fileService,
        IEmployeeNamesCache employeeNamesCache)
    {
        _userManager = userManager;
        _repository = repository;
        _fileService = fileService;
        _employeeNamesCache = employeeNamesCache;
    }

    public async Task<OperationResponse<GetEmployeeByIdQuery.Response>> Handle(ModifyEmployeeCommand.Request request, CancellationToken cancellationToken)
    {
        if (request.Image is not null &&
            !_fileService.ValidateFile(request.Image, new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp" }, 5.0))
        {
            return new HttpMessage(
                "ملف الصورة غير صالح. الصيغ المسموحة هي JPG و JPEG و PNG و GIF و WEBP و BMP، والحد الأقصى للحجم هو 5 ميغابايت.",
                HttpStatusCode.BadRequest);
        }

        request.UserName = request.UserName.ToLower();
        if (request.Email is not null)
            request.Email = request.Email.ToLower();

        var user = await _repository.TrackingQuery<AppUser>()
            .Include(e => e.UserRoles)
            .FirstOrDefaultAsync(e => e.Id == request.EmployeeId, cancellationToken);

        if (user is null)
            return EmployeeErrors.NotFound();
        if (user.UserName != null && user.UserName!.ToUpper() == "SUPERADMIN")
            return EmployeeErrors.CannotModifySuperAdmin();
        if (await _repository.Query<AppUser>()
                .AnyAsync(e => e.UserName == request.UserName && e.Id != user.Id, cancellationToken: cancellationToken))
            return EmployeeErrors.UserNameAlreadyExists(request.UserName);

        // Handle Image Upload/Delete
        var oldImageUrls = user.ImageUrl;
        if (request.DeleteImage && (oldImageUrls.OriginalImageUrl is not null || oldImageUrls.ThumbnailUrl is not null))
        {
            if (oldImageUrls.OriginalImageUrl is not null)
                await _fileService.DeleteAsync(oldImageUrls.OriginalImageUrl);
            if (oldImageUrls.ThumbnailUrl is not null)
                await _fileService.DeleteAsync(oldImageUrls.ThumbnailUrl);
            user.ClearImageUrls();
        }

        if (request.Image is not null)
        {
            var (originalUrl, thumbnailUrl) = await _fileService.UploadWithThumbnailAsync(request.Image, "Users/Images");

            if (!request.DeleteImage && (oldImageUrls.OriginalImageUrl is not null || oldImageUrls.ThumbnailUrl is not null))
            {
                if (oldImageUrls.OriginalImageUrl is not null)
                    await _fileService.DeleteAsync(oldImageUrls.OriginalImageUrl);
                if (oldImageUrls.ThumbnailUrl is not null)
                    await _fileService.DeleteAsync(oldImageUrls.ThumbnailUrl);
            }

            user.UpdateImageUrls(originalUrl, thumbnailUrl);
        }

        user.UpdateBasicInformation(
            firstName: request.FirstName,
            lastName: request.LastName,
            email: request.Email,
            userName: request.UserName!,
            phoneNumber: request.PhoneNumber);

        user.UpdatePersonalInformation(request.Gender, request.DateOfBirth);
        user.SetStatus(request.ActiveStatus);

        // Handle password change if provided
        if (!string.IsNullOrWhiteSpace(request.NewPassword))
        {
            var removePasswordResult = await _userManager.RemovePasswordAsync(user);
            if (!removePasswordResult.Succeeded)
                return new HttpMessage("فشل إعادة تعيين كلمة المرور!", HttpStatusCode.InternalServerError);
            var changePasswordResult = await _userManager.AddPasswordAsync(user, request.NewPassword);
            if (!changePasswordResult.Succeeded)
            {
                var errors = string.Join(", ", changePasswordResult.Errors.Select(e => e.Description));
                return EmployeeErrors.PasswordChangeFailed(errors);
            }
        }

        var identityResult = await _userManager.UpdateAsync(user);
        if (!identityResult.Succeeded)
        {
            var errors = string.Join(", ", identityResult.Errors.Select(e => e.Description));
            return new HttpMessage(errors, HttpStatusCode.BadRequest);
        }

        // Ensure cache stays consistent after updates.
        await _employeeNamesCache.InvalidateAsync(cancellationToken);

        var currentRoles = await _userManager.GetRolesAsync(user);
        var rolesToRemove = currentRoles.ToList();

        if (rolesToRemove.Any())
        {
            identityResult = await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
            if (!identityResult.Succeeded)
            {
                var errors = string.Join(", ", identityResult.Errors.Select(e => e.Description));
                return new HttpMessage(errors, HttpStatusCode.BadRequest);
            }
        }

        if (request.RoleIds.Any())
        {
            var roles = await _repository.Query<Domain.Entities.Security.Role>()
                .Where(r => request.RoleIds.Contains(r.Id))
                .ToListAsync(cancellationToken);

            foreach (var role in roles)
            {
                identityResult = await _userManager.AddToRoleAsync(user, role.Name!);
                if (!identityResult.Succeeded)
                {
                    var errors = string.Join(", ", identityResult.Errors.Select(e => e.Description));
                    return new HttpMessage(errors, HttpStatusCode.BadRequest);
                }
            }
        }

        return await _repository.Query<AppUser>()
            .Include(e => e.UserRoles)
            .Where(e => e.Id == request.EmployeeId)
            .OrderByDescending(e => e.DateCreated)
            .Select(GetEmployeeByIdQuery.Response.Selector())
            .FirstAsync(cancellationToken);
    }
}
