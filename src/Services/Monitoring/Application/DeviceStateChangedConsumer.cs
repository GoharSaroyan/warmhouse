using MassTransit;
using WarmHouse.Contracts;

namespace Monitoring.Api.Application;

/// <summary>
/// Consumes the state-change event the Device Gateway publishes for
/// every device type, and records it as that device's current live
/// state.
/// </summary>
public class DeviceStateChangedConsumer : IConsumer<DeviceStateChanged>
{
    private readonly MonitoringHandler _handler;
    private readonly ILogger<DeviceStateChangedConsumer> _logger;

    public DeviceStateChangedConsumer(MonitoringHandler handler, ILogger<DeviceStateChangedConsumer> logger)
    {
        _handler = handler;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<DeviceStateChanged> context)
    {
        var message = context.Message;
        var request = new ReportStateRequest(message.HouseId, message.Value, message.Status);
        await _handler.ReportAsync(message.DeviceId, request, context.CancellationToken);
        _logger.LogInformation("Recorded live state for device {DeviceId}: {Value} ({Status})", message.DeviceId, message.Value, message.Status);
    }
}
