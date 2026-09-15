using MassTransit;
using WarmHouse.Contracts;

namespace DeviceGateway.Api.Consumers;

/// <summary>
/// The only thing in the platform that actually talks to physical/
/// partner heating devices (see docs/c4/container-to-be.puml). Consumes
/// a command requested by the Heating Control Service, "delivers" it,
/// then publishes the ack and the resulting state change.
///
/// TODO: replace the simulated delivery below with a real call out to
/// hardware over its standard protocol (MQTT, etc.) once real devices
/// exist to talk to.
/// </summary>
public class HeatingCommandRequestedConsumer : IConsumer<HeatingCommandRequested>
{
    private readonly ILogger<HeatingCommandRequestedConsumer> _logger;

    public HeatingCommandRequestedConsumer(ILogger<HeatingCommandRequestedConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<HeatingCommandRequested> context)
    {
        var message = context.Message;
        _logger.LogInformation("Delivering heating command to device {DeviceId}: {Value}", message.DeviceId, message.DesiredValue);

        // Simulated hardware round-trip - the device did what it was told.
        var actualValue = message.DesiredValue;

        await context.Publish(new HeatingCommandCompleted(message.DeviceId, actualValue));
        await context.Publish(new DeviceStateChanged(message.DeviceId, message.HouseId, actualValue, "active"));
    }
}
