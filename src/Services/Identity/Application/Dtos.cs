using Identity.Api.Domain;

namespace Identity.Api.Application;

// PasswordHash is deliberately not exposed here.
public record UserResponse(Guid Id, string Name, string Email, DateTimeOffset CreatedAt)
{
    public static UserResponse From(User u) => new(u.Id, u.Name, u.Email, u.CreatedAt);
}

public record RegisterUserRequest(string Name, string Email, string Password);

public record HouseResponse(Guid Id, Guid UserId, string Address, DateTimeOffset CreatedAt)
{
    public static HouseResponse From(House h) => new(h.Id, h.UserId, h.Address, h.CreatedAt);
}

public record CreateHouseRequest(Guid UserId, string Address);
