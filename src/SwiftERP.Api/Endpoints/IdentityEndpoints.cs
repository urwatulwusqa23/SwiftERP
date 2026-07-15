using MediatR;
using SwiftERP.Api.Authorization;
using SwiftERP.Identity.Application.Auth.Login;
using SwiftERP.Identity.Application.Roles.CreateRole;
using SwiftERP.Identity.Application.Roles.GetRoles;
using SwiftERP.Identity.Application.Roles.UpdateRolePermission;
using SwiftERP.Identity.Application.Users.AssignRole;
using SwiftERP.Identity.Application.Users.CreateUserAccount;
using SwiftERP.Identity.Application.Users.GetUsers;
using SwiftERP.Identity.Application.Users.RemoveRole;

namespace SwiftERP.Api.Endpoints;

public static class IdentityEndpoints
{
    public static void MapIdentityEndpoints(this IEndpointRouteBuilder app)
    {
        // Login is the one endpoint in the whole Api that must be reachable unauthenticated.
        app.MapPost("/api/v1/auth/login", async (LoginCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.Json(new { error = result.Error }, statusCode: StatusCodes.Status401Unauthorized);
        }).WithTags("Auth").AllowAnonymous();

        var roles = app.MapGroup("/api/v1/admin/roles").WithTags("Admin - Roles").RequireSystemAdmin();

        roles.MapGet("/", async (ISender sender) =>
            Results.Ok(await sender.Send(new GetRolesQuery())));

        roles.MapPost("/", async (CreateRoleCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return result.IsSuccess
                ? Results.Created($"/api/v1/admin/roles/{result.Value}", new { id = result.Value })
                : Results.BadRequest(new { error = result.Error });
        });

        roles.MapPut("/{roleId:guid}/permissions", async (Guid roleId, UpdateRolePermissionRequest request, ISender sender) =>
        {
            var result = await sender.Send(new UpdateRolePermissionCommand(roleId, request.Module, request.AccessLevel));
            return result.IsSuccess ? Results.NoContent() : Results.BadRequest(new { error = result.Error });
        });

        var users = app.MapGroup("/api/v1/admin/users").WithTags("Admin - Users").RequireSystemAdmin();

        users.MapGet("/", async (ISender sender) =>
            Results.Ok(await sender.Send(new GetUsersQuery())));

        users.MapPost("/", async (CreateUserAccountCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return result.IsSuccess
                ? Results.Created($"/api/v1/admin/users/{result.Value}", new { id = result.Value })
                : Results.BadRequest(new { error = result.Error });
        });

        users.MapPost("/{userId:guid}/roles", async (Guid userId, AssignRoleRequest request, ISender sender) =>
        {
            var result = await sender.Send(new AssignRoleCommand(userId, request.RoleId));
            return result.IsSuccess ? Results.NoContent() : Results.BadRequest(new { error = result.Error });
        });

        users.MapDelete("/{userId:guid}/roles/{roleId:guid}", async (Guid userId, Guid roleId, ISender sender) =>
        {
            var result = await sender.Send(new RemoveRoleCommand(userId, roleId));
            return result.IsSuccess ? Results.NoContent() : Results.BadRequest(new { error = result.Error });
        });
    }
}

public record UpdateRolePermissionRequest(SwiftERP.Identity.Domain.Roles.Module Module, SwiftERP.Identity.Domain.Roles.AccessLevel AccessLevel);
public record AssignRoleRequest(Guid RoleId);
