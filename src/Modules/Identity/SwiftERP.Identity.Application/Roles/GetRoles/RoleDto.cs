namespace SwiftERP.Identity.Application.Roles.GetRoles;

public record ModulePermissionDto(string Module, string AccessLevel);

public record RoleDto(Guid Id, string Name, bool IsSystemRole, List<ModulePermissionDto> Permissions);
