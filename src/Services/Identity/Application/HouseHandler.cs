using Identity.Api.Domain;

namespace Identity.Api.Application;

public class HouseHandler
{
    private readonly IHouseRepository _houses;
    private readonly IUserRepository _users;

    public HouseHandler(IHouseRepository houses, IUserRepository users)
    {
        _houses = houses;
        _users = users;
    }

    public Task<House?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return _houses.GetByIdAsync(id, ct);
    }

    public Task<List<House>> GetForUserAsync(Guid userId, CancellationToken ct = default)
    {
        return _houses.GetForUserAsync(userId, ct);
    }

    public async Task<House> CreateAsync(CreateHouseRequest request, CancellationToken ct = default)
    {
        if (await _users.GetByIdAsync(request.UserId, ct) is null)
        {
            throw new InvalidOperationException($"User '{request.UserId}' does not exist");
        }

        var house = new House
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            Address = request.Address,
            CreatedAt = DateTimeOffset.UtcNow,
        };

        return await _houses.CreateAsync(house, ct);
    }
}
