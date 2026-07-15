using MediatR;

namespace SwiftERP.Identity.Application.Users.GetUsers;

public record GetUsersQuery : IRequest<List<UserDto>>;
