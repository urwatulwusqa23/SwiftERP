using SwiftERP.Identity.Application.Abstractions;
using SwiftERP.Identity.Domain.Roles;

namespace SwiftERP.Api.Authorization;

/// <summary>
/// Reads the swifterp:module:{Module} claim baked into the JWT at login and rejects the request
/// if the caller's effective access level for that module is below what the route requires.
/// </summary>
public class RequireModuleAccessFilter(Module module, AccessLevel minLevel) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var user = context.HttpContext.User;
        if (user.Identity?.IsAuthenticated != true)
            return Results.Json(new { error = "Authentication required." }, statusCode: StatusCodes.Status401Unauthorized);

        if (!HasAccess(user, module, minLevel))
            return Results.Json(new { error = $"Requires {minLevel} access to {module}." }, statusCode: StatusCodes.Status403Forbidden);

        return await next(context);
    }

    public static bool HasAccess(System.Security.Claims.ClaimsPrincipal user, Module module, AccessLevel minLevel)
    {
        var claimValue = user.FindFirst(SwiftErpClaimTypes.ModuleClaim(module.ToString()))?.Value;
        return Enum.TryParse<AccessLevel>(claimValue, out var level) && level >= minLevel;
    }
}

/// <summary>
/// Allows the request through if either (a) the caller's swifterp:employee_id claim matches the
/// {employeeIdRouteParam} route value — self-service — or (b) they hold at least minLevel on
/// module. Used for HR endpoints like "view my own attendance/leave/documents" where an
/// employee should not need HR module access just to see their own data.
/// </summary>
public class RequireSelfOrModuleAccessFilter(Module module, AccessLevel minLevel, string employeeIdRouteParam = "id")
    : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var user = context.HttpContext.User;
        if (user.Identity?.IsAuthenticated != true)
            return Results.Json(new { error = "Authentication required." }, statusCode: StatusCodes.Status401Unauthorized);

        var routeEmployeeId = context.HttpContext.Request.RouteValues[employeeIdRouteParam]?.ToString();
        var callerEmployeeId = user.FindFirst(SwiftErpClaimTypes.EmployeeId)?.Value;

        var isSelf = routeEmployeeId is not null
            && callerEmployeeId is not null
            && Guid.TryParse(routeEmployeeId, out var routeGuid)
            && Guid.TryParse(callerEmployeeId, out var callerGuid)
            && routeGuid == callerGuid;

        if (!isSelf && !RequireModuleAccessFilter.HasAccess(user, module, minLevel))
            return Results.Json(new { error = $"Requires {minLevel} access to {module}, or must be your own record." }, statusCode: StatusCodes.Status403Forbidden);

        return await next(context);
    }
}

/// <summary>
/// Gates the Identity module's own role/user-management endpoints — these don't map to one of
/// the four business modules, so access is a flat "holds a system role" check via the
/// swifterp:is_system_admin claim rather than a per-module access level.
/// </summary>
public class RequireSystemAdminFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var user = context.HttpContext.User;
        if (user.Identity?.IsAuthenticated != true)
            return Results.Json(new { error = "Authentication required." }, statusCode: StatusCodes.Status401Unauthorized);

        if (user.FindFirst(SwiftErpClaimTypes.IsSystemAdmin)?.Value != "true")
            return Results.Json(new { error = "Requires a system admin role." }, statusCode: StatusCodes.Status403Forbidden);

        return await next(context);
    }
}

public static class ModuleAccessFilterExtensions
{
    public static TBuilder RequireModule<TBuilder>(this TBuilder builder, Module module, AccessLevel minLevel)
        where TBuilder : IEndpointConventionBuilder
    {
        builder.AddEndpointFilter(new RequireModuleAccessFilter(module, minLevel));
        builder.RequireAuthorization();
        return builder;
    }

    public static TBuilder RequireSelfOrModule<TBuilder>(
        this TBuilder builder, Module module, AccessLevel minLevel, string employeeIdRouteParam = "id")
        where TBuilder : IEndpointConventionBuilder
    {
        builder.AddEndpointFilter(new RequireSelfOrModuleAccessFilter(module, minLevel, employeeIdRouteParam));
        builder.RequireAuthorization();
        return builder;
    }

    public static TBuilder RequireSystemAdmin<TBuilder>(this TBuilder builder)
        where TBuilder : IEndpointConventionBuilder
    {
        builder.AddEndpointFilter(new RequireSystemAdminFilter());
        builder.RequireAuthorization();
        return builder;
    }
}
