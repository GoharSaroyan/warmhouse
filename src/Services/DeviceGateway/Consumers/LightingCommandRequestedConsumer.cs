using MassTransit;
using WarmHouse.Contracts;

namespace DeviceGateway.Api.Consumers;

/// <summary>
/// The only thing in the platform that actually talks to physical/
/// partner lighting devices. See HeatingCommandRequestedConsumer for the
/// same pattern and its TODO on real hardware delivery.
/// </summary>
public class LightingCommandRequestedConsumer : IConsumer<LightingCommandRequested>
{
    private readonly ILogger<LightingCommandRequestedConsumer> _logger;

    public LightingCommandRequestedConsumer(ILogger<LightingCommandRequestedConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<LightingCommandRequested> context)
    {
        var message = context.Message;
        _logger.LogInformation("Delivering lighting command to device {DeviceId}: {Value}", message.DeviceId, message.DesiredValue);

        var actualValue = message.DesiredValue;

        await context.Publish(new LightingCommandCompleted(message.DeviceId, actualValue));
        await context.Publish(new DeviceStateChanged(message.DeviceId, message.HouseId, actualValue, "active"));
    }
}
