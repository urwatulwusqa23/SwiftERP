namespace SwiftERP.Identity.Application.Abstractions;

/// <summary>
/// Shared between the token issuer (writes these claims into the JWT at login) and the Api
/// layer's authorization filter (reads them back out on every request) — kept as constants here
/// so the two sides can't drift out of sync on the claim type strings.
/// </summary>
public static class SwiftErpClaimTypes
{
    public const string EmployeeId = "swifterp:employee_id";
    public const string ModulePermissionPrefix = "swifterp:module:";
    public const string IsSystemAdmin = "swifterp:is_system_admin";

    public static string ModuleClaim(string module) => $"{ModulePermissionPrefix}{module}";
}
