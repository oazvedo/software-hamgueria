namespace SoftwareHamgueria.Application.Messaging;

public interface IOrderEventPublisher
{
    void Publish(string routingKey, string message);
}
