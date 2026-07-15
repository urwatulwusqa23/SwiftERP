using MediatR;
using SwiftERP.SharedKernel;

namespace SwiftERP.Identity.Application.Auth.Login;

public record LoginCommand(string Email, string Password) : IRequest<Result<LoginResponse>>;

public record LoginResponse(
    string Token,
    DateTimeOffset ExpiresAtUtc,
    Guid UserId,
    Guid EmployeeId,
    string Email,
    List<string> Roles,
    Dictionary<string, string> Permissions);
