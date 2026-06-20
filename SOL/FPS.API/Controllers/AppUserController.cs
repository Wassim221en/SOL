using FPS.Dashboard.Dashboard.Features.Employee.Commands.Add;
using FPS.Dashboard.Dashboard.Features.Employee.Commands.Auth.Commands.CodeVerification;
using FPS.Dashboard.Dashboard.Features.Employee.Commands.Auth.Commands.EmailConfirmed;
using FPS.Dashboard.Dashboard.Features.Employee.Commands.Auth.Commands.ForgetPassword;
using FPS.Dashboard.Dashboard.Features.Employee.Commands.Auth.Commands.Login;
using FPS.Dashboard.Dashboard.Features.Employee.Commands.Auth.Commands.RefreshToken;
using FPS.Dashboard.Dashboard.Features.Employee.Commands.Auth.Commands.ResetPassword;
using FPS.Dashboard.Dashboard.Features.Employee.Commands.Delete;
using FPS.Dashboard.Dashboard.Features.Employee.Commands.Modify;
using FPS.Dashboard.Dashboard.Features.Employee.Commands.ModifyImage;
using FPS.Dashboard.Dashboard.Features.Employee.Commands.ModifyMyPassword;
using FPS.Dashboard.Dashboard.Features.Employee.Commands.ModifyMyProfile;
using FPS.Dashboard.Dashboard.Features.Employee.Queries.GetAll;
using FPS.Dashboard.Dashboard.Features.Employee.Queries.GetAllName;
using FPS.Dashboard.Dashboard.Features.Employee.Queries.GetById;
using FPS.Dashboard.Dashboard.Features.Employee.Queries.GetEmployeeCardAsPdf;
using FPS.Dashboard.Dashboard.Features.Employee.Queries.GetEmployeesAsPdf;
using FPS.Dashboard.Dashboard.Features.Employee.Queries.GetMyProfile;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Template.API;
using Template.API.Attributes;
using Template.API.Authorization;
using Template.API.Extensions;
using Template.Application.Common.Core.Response;

[ApiController]
[Route("api/[controller]/[action]")]
[ApiGroup<SampleApiGroup>(SampleApiGroup.Dashboard)]
public class AppUserController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<AppUserController> _logger;

    public AppUserController(IMediator mediator, ILogger<AppUserController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [AllowAnonymous]
    [HttpPost]
    public async Task<IActionResult> Login(
        [FromBody] LoginCommand.Request request,
        [FromServices] IRequestHandler<LoginCommand.Request, OperationResponse<LoginCommand.Response>> handler)
        => (await handler.Handle(request, CancellationToken.None)).ToActionResult();

    /// <summary>
    /// Default login endpoint for Swagger testing with predefined admin credentials
    /// </summary>
    [AllowAnonymous]
    [HttpPost("DefaultLogin")]
    public async Task<IActionResult> DefaultLogin(
        [FromServices] IRequestHandler<LoginCommand.Request, OperationResponse<LoginCommand.Response>> handler)
    {
        var defaultRequest = new LoginCommand.Request
        {
            Email = "admin@template.com",
            Password = "Admin@123"
        };
        
        return (await handler.Handle(defaultRequest, CancellationToken.None)).ToActionResult();
    }


    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> ForgetPassword(
        [FromBody] ForgetPasswordCommand.Request request,
        [FromServices] IRequestHandler<ForgetPasswordCommand.Request, OperationResponse> handler)
        => (await handler.Handle(request, CancellationToken.None)).ToActionResult();


    [HttpPost]
    [AllowAnonymous]
    [ProducesResponseType(typeof(OperationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword(
        [FromBody] ResetPasswordCommand.Request request,
        [FromServices] IRequestHandler<ResetPasswordCommand.Request, OperationResponse<LoginCommand.Response>> handler)
        => (await handler.Handle(request, CancellationToken.None)).ToActionResult();


    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> RefreshToken(
        [FromBody] RefreshTokenCommand.Request request,
        [FromServices]
        IRequestHandler<RefreshTokenCommand.Request, OperationResponse<LoginCommand.Response>> handler)
        => (await handler.Handle(request, CancellationToken.None)).ToActionResult();


    [HttpGet]
    [HasPermissions(PermissionOperator.Or,Permissions.Employees.View,Permissions.HR.View)]
    [ProducesResponseType(typeof(OperationResponse<GetAllEmployeesQuery.Response>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAll(
        [FromQuery] GetAllEmployeesQuery.Request request,
        [FromServices] IRequestHandler<GetAllEmployeesQuery.Request, OperationResponse<GetAllEmployeesQuery.Response>> handler)
        => (await handler.Handle(request, CancellationToken.None)).ToActionResult();


    [HttpGet]
    [ProducesResponseType(typeof(OperationResponse<List<GetAllEmployeeNamesQuery.Response>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAllNames(
        [FromQuery] GetAllEmployeeNamesQuery.Request request,
        [FromServices] IRequestHandler<GetAllEmployeeNamesQuery.Request, OperationResponse<List<GetAllEmployeeNamesQuery.Response>>> handler)
        => (await handler.Handle(request, CancellationToken.None)).ToActionResult();


    [HttpGet]
    [HasPermissions(Permissions.Employees.View)]
    [ProducesResponseType(typeof(OperationResponse<GetEmployeeByIdQuery.Response>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        [FromQuery] GetEmployeeByIdQuery.Request request,
        [FromServices] IRequestHandler<GetEmployeeByIdQuery.Request, OperationResponse<GetEmployeeByIdQuery.Response>> handler)
        => (await handler.Handle(request, CancellationToken.None)).ToActionResult();


    [HttpPost]
    [HasPermissions(Permissions.Employees.Create)]
    [ProducesResponseType(typeof(OperationResponse<GetAllEmployeesQuery.Response.EmployeeRes>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Add(
        [FromForm] AddEmployeeCommand.Request request,
        [FromServices]
        IRequestHandler<AddEmployeeCommand.Request, OperationResponse<GetAllEmployeesQuery.Response.EmployeeRes>> handler)
        => (await handler.Handle(request, CancellationToken.None)).ToActionResult();


    [HttpPost]
    [HasPermissions(PermissionOperator.Or,Permissions.Employees.Update,Permissions.Employees.Create)]
    [ProducesResponseType(typeof(OperationResponse<GetEmployeeByIdQuery.Response>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Modify(
        [FromForm] ModifyEmployeeCommand.Request request,
        [FromServices]
        IRequestHandler<ModifyEmployeeCommand.Request, OperationResponse<GetEmployeeByIdQuery.Response>> handler)
        => (await handler.Handle(request, CancellationToken.None)).ToActionResult();


    [HttpDelete]
    [HasPermissions(Permissions.Employees.Delete)]
    [ProducesResponseType(typeof(OperationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        [FromQuery]Guid ?id,
        [FromQuery]List<Guid>? ids,
        [FromServices]
        IRequestHandler<DeleteEmployeesCommand.Request, OperationResponse> handler)
        => (await handler.Handle(new(id, ids ?? new List<Guid>()), CancellationToken.None)).ToActionResult();

    [HttpGet]
    [ProducesResponseType(typeof(OperationResponse<GetMyProfileQuery.Response>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMyProfile([FromQuery] GetMyProfileQuery.Request request,
        [FromServices]
        IRequestHandler<GetMyProfileQuery.Request, OperationResponse<GetMyProfileQuery.Response>> handler)
        => (await handler.Handle(request, CancellationToken.None)).ToActionResult();

    
    [HttpPost]
    [ProducesResponseType(typeof(OperationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ResetTokenVerification([FromBody] CodeVerificationCommand.Request request,
        [FromServices] IRequestHandler<CodeVerificationCommand.Request, OperationResponse> handler)
        => (await handler.Handle(request, CancellationToken.None)).ToActionResult();

    [HttpPost]
    [HasPermissions(Permissions.Employees.View)]
    [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetEmployeesAsPdf(
        [FromBody] GetEmployeesAsPdfQuery.Request request,
        [FromServices] IRequestHandler<GetEmployeesAsPdfQuery.Request, OperationResponse<GetEmployeesAsPdfQuery.Response>> handler)
    {
        var response = await handler.Handle(request, CancellationToken.None);

        if (!response.Success || response.Data == null)
        {
            return BadRequest(response);
        }

        return File(
            response.Data.PdfBytes,
            "application/pdf",
            response.Data.FileName
        );
    }

    [HttpGet]
   // [HasPermissions(Permissions.Employees.View)]
    [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetEmployeeCardAsPdf(
        [FromQuery] GetEmployeeCardAsPdfQuery.Request request,
        [FromServices] IRequestHandler<GetEmployeeCardAsPdfQuery.Request, OperationResponse<GetEmployeeCardAsPdfQuery.Response>> handler)
    {
        var response = await handler.Handle(request, CancellationToken.None);

        if (!response.Success || response.Data == null)
        {
            return response.ToActionResult();
        }

        return File(
            response.Data.PdfBytes,
            "application/pdf",
            response.Data.FileName
        );
    }
    [HttpPost]
    public async Task<IActionResult> EmailConfirmed([FromQuery]EmailConfirmedCommand.Request request,
        [FromServices]IRequestHandler<EmailConfirmedCommand.Request,OperationResponse>handler)
    =>(await handler.Handle(request,CancellationToken.None)).ToActionResult();
    
    [HttpPost]
    public async Task<IActionResult> ModifyMyProfile(
        [FromForm] ModifyMyProfileCommand.Request request,
        [FromServices] IRequestHandler<ModifyMyProfileCommand.Request, OperationResponse<ModifyMyProfileCommand.Response>> handler)
        => (await handler.Handle(request, CancellationToken.None)).ToActionResult();
    
    [HttpPost]
    public async Task<IActionResult> ModifyMyPassword([FromBody] ModifyMyPasswordCommand.Request request,
        [FromServices] IRequestHandler<ModifyMyPasswordCommand.Request, OperationResponse> handler)
        => (await handler.Handle(request, CancellationToken.None)).ToActionResult();
    
    
    [HttpPost]
    [HasPermissions(PermissionOperator.Or,Permissions.Employees.Update,Permissions.Employees.Create)]
    public async Task<IActionResult>ModifyPersonalImage([FromForm]ModifyImageCommand.Request request,
        [FromServices]IRequestHandler<ModifyImageCommand.Request,OperationResponse<ModifyImageCommand.Response>>handler
        )=>(await handler.Handle(request,CancellationToken.None)).ToActionResult();
    
    
}
