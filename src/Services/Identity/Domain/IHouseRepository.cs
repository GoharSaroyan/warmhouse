namespace Identity.Api.Domain;

public interface IHouseRepository
{
    Task<House?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<House>> GetForUserAsync(Guid userId, CancellationToken ct = default);
    Task<House> CreateAsync(House house, CancellationToken ct = default);
}
