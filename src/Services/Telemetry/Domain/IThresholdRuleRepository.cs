namespace Telemetry.Api.Domain;

public interface IThresholdRuleRepository
{
    Task<List<ThresholdRule>> GetApplicableRulesAsync(Guid houseId, Guid deviceId, string metric, CancellationToken ct = default);
    Task<List<ThresholdRule>> GetForHouseAsync(Guid houseId, CancellationToken ct = default);
    Task<ThresholdRule> CreateAsync(ThresholdRule rule, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
}
