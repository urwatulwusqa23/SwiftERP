namespace SwiftERP.Identity.Application.Users.GetUsers;

public record UserDto(
    Guid Id,
    Guid EmployeeId,
    string EmployeeName,
    string Email,
    bool IsActive,
    List<Guid> RoleIds,
    List<string> RoleNames);
