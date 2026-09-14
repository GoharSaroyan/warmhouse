using MassTransit;
using WarmHouse.Contracts;

namespace Lighting.Api.Application;

/// <summary>
/// Consumes the ack the Device Gateway publishes once a lighting command
/// has actually been delivered.
/// </summary>
public class LightingCommandCompletedConsumer : IConsumer<LightingCommandCompleted>
{
    private readonly LightingCommandHandler _handler;
    private readonly ILogger<LightingCommandCompletedConsumer> _logger;

    public LightingCommandCompletedConsumer(LightingCommandHandler handler, ILogger<LightingCommandCompletedConsumer> logger)
    {
        _handler = handler;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<LightingCommandCompleted> context)
    {
        var message = context.Message;
        await _handler.ReportActualAsync(message.DeviceId, message.ActualValue, context.CancellationToken);
        _logger.LogInformation("Recorded actual lighting value for device {DeviceId}: {Value}", message.DeviceId, message.ActualValue);
    }
}
