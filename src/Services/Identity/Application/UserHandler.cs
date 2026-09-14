using Identity.Api.Domain;
using Identity.Api.Infrastructure;

namespace Identity.Api.Application;

public class UserHandler
{
    private readonly IUserRepository _repository;

    public UserHandler(IUserRepository repository) => _repository = repository;

    public Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return _repository.GetByIdAsync(id, ct);
    }

    public async Task<User> RegisterAsync(RegisterUserRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            throw new ArgumentException("email and password are required");
        }

        if (await _repository.GetByEmailAsync(request.Email, ct) is not null)
        {
            throw new InvalidOperationException("A user with this email already exists");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Email = request.Email,
            PasswordHash = PasswordHasher.Hash(request.Password),
            CreatedAt = DateTimeOffset.UtcNow,
        };

        return await _repository.CreateAsync(user, ct);
    }
}
