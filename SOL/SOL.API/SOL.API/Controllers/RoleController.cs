using SOL.Dashboard.Dashboard.Features.Role.Commands.Add;
using SOL.Dashboard.Dashboard.Features.Role.Commands.Delete;
using SOL.Dashboard.Dashboard.Features.Role.Commands.Modify;
using SOL.Dashboard.Dashboard.Features.Role.Queries.GetAll;
using SOL.Dashboard.Dashboard.Features.Role.Queries.GetById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Template.API;
using Template.API.Attributes;
using Template.API.Extensions;
using Template.Application.Common.Core.Response;

[ApiController]
[Route("api/[controller]/[action]")]
[ApiGroup<SampleApiGroup>(SampleApiGroup.Dashboard)]
public class RoleController : ControllerBase
{
    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Add(
        [FromBody] AddRoleCommand.Request request,
        [FromServices] IRequestHandler<AddRoleCommand.Request, OperationResponse<GetAllRolesQuery.Response.RoleRes>> handler)
        => (await handler.Handle(request, CancellationToken.None)).ToActionResult();

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Modify(
        [FromBody] ModifyRoleCommand.Request request,
        [FromServices] IRequestHandler<ModifyRoleCommand.Request, OperationResponse<GetRolebyIdQuery.Response>> handler)
        => (await handler.Handle(request, CancellationToken.None)).ToActionResult();

    [HttpDelete]
    [AllowAnonymous]
    public async Task<IActionResult> Delete(
        [FromQuery] Guid id,
        [FromServices] IRequestHandler<DeleteRolesCommand.Request, OperationResponse> handler)
        => (await handler.Handle(new DeleteRolesCommand.Request { RoleId = id }, CancellationToken.None)).ToActionResult();

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll(
        [FromQuery] GetAllRolesQuery.Request request,
        [FromServices] IRequestHandler<GetAllRolesQuery.Request, OperationResponse<GetAllRolesQuery.Response>> handler)
        => (await handler.Handle(request, CancellationToken.None)).ToActionResult();

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(
        [FromQuery] GetRolebyIdQuery.Request request,
        [FromServices] IRequestHandler<GetRolebyIdQuery.Request, OperationResponse<GetRolebyIdQuery.Response>> handler)
        => (await handler.Handle(request, CancellationToken.None)).ToActionResult();
}