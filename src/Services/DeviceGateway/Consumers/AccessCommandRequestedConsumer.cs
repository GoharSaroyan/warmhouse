using MassTransit;
using WarmHouse.Contracts;

namespace DeviceGateway.Api.Consumers;

/// <summary>
/// The only thing in the platform that actually talks to physical/
/// partner gate devices. See HeatingCommandRequestedConsumer for the
/// same pattern and its TODO on real hardware delivery.
/// </summary>
public class AccessCommandRequestedConsumer : IConsumer<AccessCommandRequested>
{
    private readonly ILogger<AccessCommandRequestedConsumer> _logger;

    public AccessCommandRequestedConsumer(ILogger<AccessCommandRequestedConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<AccessCommandRequested> context)
    {
        var message = context.Message;
        _logger.LogInformation("Delivering gate command to device {DeviceId}: {Value} (requested by {Actor})", message.DeviceId, message.DesiredValue, message.Actor);

        var actualValue = message.DesiredValue;

        await context.Publish(new AccessCommandCompleted(message.DeviceId, actualValue));
        await context.Publish(new DeviceStateChanged(message.DeviceId, message.HouseId, actualValue, "active"));
    }
}
