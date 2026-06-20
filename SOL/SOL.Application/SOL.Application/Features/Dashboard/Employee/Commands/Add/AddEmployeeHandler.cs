using System.Net;
using SOL.Dashboard.Dashboard.Features.Employee.Queries.GetAll;
using SOL.Domain.Entities.Security;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace SOL.Dashboard.Dashboard.Features.Employee.Commands.Add;

public class AddEmployeeHandler : IRequestHandler<AddEmployeeCommand.Request, OperationResponse<GetAllEmployeesQuery.Response.EmployeeRes>>
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IRepository _repository;
    private readonly IEventBus _eventBus;
    private readonly IFileService _fileService;
    private readonly IEmployeeNamesCache _employeeNamesCache;

    public AddEmployeeHandler(
        UserManager<AppUser> userManager,
        IRepository repository,
        IEventBus eventBus,
        IFileService fileService,
        IEmployeeNamesCache employeeNamesCache)
    {
        _userManager = userManager;
        _repository = repository;
        _eventBus = eventBus;
        _fileService = fileService;
        _employeeNamesCache = employeeNamesCache;
    }

    public async Task<OperationResponse<GetAllEmployeesQuery.Response.EmployeeRes>> Handle(AddEmployeeCommand.Request request, CancellationToken cancellationToken)
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

        var isUserNameExists = await _repository.Query<AppUser>()
            .AnyAsync(e => e.UserName == request.UserName, cancellationToken: cancellationToken);
        if (isUserNameExists)
            return EmployeeErrors.UserNameAlreadyExists(request.UserName);

        if (request.Email is not null && await _repository.Query<AppUser>()
                .AnyAsync(e => e.Email != null && e.Email == request.Email, cancellationToken: cancellationToken))
            return EmployeeErrors.EmailAlreadyExists(request.Email);

        if (await _repository.Query<AppUser>()
                .AnyAsync(e => e.PhoneNumber == request.PhoneNumber, cancellationToken: cancellationToken))
            return EmployeeErrors.PhoneNumberAlreadyExists(request.PhoneNumber);

        string? originalImageUrl = null;
        string? thumbnailUrl = null;

        if (request.Image is not null)
        {
            var (original, thumbnail) = await _fileService.UploadWithThumbnailAsync(request.Image, "Users/Images");
            originalImageUrl = original;
            thumbnailUrl = thumbnail;
        }

        var user = new AppUser();

        user.UpdateBasicInformation(
            firstName: request.FirstName,
            lastName: request.LastName,
            email: request.Email,
            userName: request.UserName,
            phoneNumber: request.PhoneNumber);

        user.UpdatePersonalInformation(request.Gender, request.DateOfBirth);
        user.SetStatus(request.ActiveStatus);

        if (originalImageUrl is not null || thumbnailUrl is not null)
            user.UpdateImageUrl(originalImageUrl, thumbnailUrl);

        var identityResult = await _userManager.CreateAsync(user, request.Password);
        if (!identityResult.Succeeded)
        {
            var errors = string.Join(", ", identityResult.Errors.Select(e => e.Description));
            if (originalImageUrl is not null)
                await _fileService.DeleteAsync(originalImageUrl);
            if (thumbnailUrl is not null)
                await _fileService.DeleteAsync(thumbnailUrl);
            return new HttpMessage(errors, HttpStatusCode.BadRequest);
        }

        // Ensure cache stays consistent after inserts.
        await _employeeNamesCache.InvalidateAsync(cancellationToken);

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

        var integrationEvent = new EmployeeCreatedIntegrationEvent(
            user.Id,
            user.FirstName,
            user.LastName,
            user.Email!,
            user.PhoneNumber);

        await _eventBus.PublishAsync(integrationEvent, cancellationToken);

        return new GetAllEmployeesQuery.Response.EmployeeRes
        {
            EmployeeId = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            FullName = user.FullName,
            UserName = user.UserName ?? "",
            Email = user.Email ?? "",
            PhoneNumber = user.PhoneNumber ?? "",
            Status = user.Status
        };
    }
}
