using MassTransit;
using WarmHouse.Contracts;

namespace Heating.Api.Application;

/// <summary>
/// Consumes the ack the Device Gateway publishes once a heating command
/// has actually been delivered, and records it as the device's actual
/// (as opposed to desired) value.
/// </summary>
public class HeatingCommandCompletedConsumer : IConsumer<HeatingCommandCompleted>
{
    private readonly HeatingCommandHandler _handler;
    private readonly ILogger<HeatingCommandCompletedConsumer> _logger;

    public HeatingCommandCompletedConsumer(HeatingCommandHandler handler, ILogger<HeatingCommandCompletedConsumer> logger)
    {
        _handler = handler;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<HeatingCommandCompleted> context)
    {
        var message = context.Message;
        await _handler.ReportActualAsync(message.DeviceId, message.ActualValue, context.CancellationToken);
        _logger.LogInformation("Recorded actual heating value for device {DeviceId}: {Value}", message.DeviceId, message.ActualValue);
    }
}
