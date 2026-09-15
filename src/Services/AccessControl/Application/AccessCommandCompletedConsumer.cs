using MassTransit;
using WarmHouse.Contracts;

namespace AccessControl.Api.Application;

/// <summary>
/// Consumes the ack the Device Gateway publishes once a gate command has
/// actually been delivered.
/// </summary>
public class AccessCommandCompletedConsumer : IConsumer<AccessCommandCompleted>
{
    private readonly AccessCommandHandler _handler;
    private readonly ILogger<AccessCommandCompletedConsumer> _logger;

    public AccessCommandCompletedConsumer(AccessCommandHandler handler, ILogger<AccessCommandCompletedConsumer> logger)
    {
        _handler = handler;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<AccessCommandCompleted> context)
    {
        var message = context.Message;
        await _handler.ReportActualAsync(message.DeviceId, message.ActualValue, context.CancellationToken);
        _logger.LogInformation("Recorded actual gate value for device {DeviceId}: {Value}", message.DeviceId, message.ActualValue);
    }
}
