using SOL.Domain.Entities.Security;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace SOL.Dashboard.Dashboard.Features.Employee.Commands.ModifyImage;

public class ModifyImageHandler 
    : IRequestHandler<ModifyImageCommand.Request, OperationResponse<ModifyImageCommand.Response>>
{
    private readonly IRepository _repository;
    private readonly IFileService _fileService;
    private readonly IEmployeeNamesCache _employeeNamesCache;

    public ModifyImageHandler(
        IRepository repository,
        IFileService fileService,
        IEmployeeNamesCache employeeNamesCache)
    {
        _repository = repository;
        _fileService = fileService;
        _employeeNamesCache = employeeNamesCache;
    }

    public async Task<OperationResponse<ModifyImageCommand.Response>> Handle(
        ModifyImageCommand.Request request,
        CancellationToken cancellationToken)
    {
       
        var employee = await _repository.TrackingQuery<AppUser>()
            .FirstOrDefaultAsync(e => e.Id == request.EmployeeId, cancellationToken);

        if (employee is null)
            return EmployeeErrors.NotFound();

        var oldImageUrls = employee.ImageUrl;

        // Delete image if requested
        if ((request.DeleteImage||request.Image is null) && (oldImageUrls.OriginalImageUrl is not null || oldImageUrls.ThumbnailUrl is not null))
        {
            if(oldImageUrls.OriginalImageUrl is not null)
                await _fileService.DeleteAsync(oldImageUrls.OriginalImageUrl);
            if(oldImageUrls.ThumbnailUrl is not null)
                await _fileService.DeleteAsync(oldImageUrls.ThumbnailUrl);
            employee.ClearImageUrls();
        }

        // Upload new image if provided
        if (request.Image is not null)
        {
            var (originalUrl, thumbnailUrl) = await _fileService.UploadWithThumbnailAsync(request.Image, "Employees/Images");

            if (!request.DeleteImage && (oldImageUrls.OriginalImageUrl is not null || oldImageUrls.ThumbnailUrl is not null))
            {
                if(oldImageUrls.OriginalImageUrl is not null)
                    await _fileService.DeleteAsync(oldImageUrls.OriginalImageUrl);
                if(oldImageUrls.ThumbnailUrl is not null)
                    await _fileService.DeleteAsync(oldImageUrls.ThumbnailUrl);
            }

            employee.UpdateImageUrls(originalUrl, thumbnailUrl);
        }

        _repository.Update(employee);
        await _repository.SaveChangesAsync(cancellationToken);

        // Ensure chat employee directory cache stays consistent after image changes.
        await _employeeNamesCache.InvalidateAsync(cancellationToken);

        return OperationResponse<ModifyImageCommand.Response>.Ok(
            new ModifyImageCommand.Response
            {
                ImageUrl = employee.ImageUrl.OriginalImageUrl,
                ThumbnailUrl = employee.ImageUrl.ThumbnailUrl,
            });
    }
}
