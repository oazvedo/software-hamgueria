using SoftwareHamgueria.Application.Messaging;
using SoftwareHamgueria.Domain.Entities;
using SoftwareHamgueria.Infrastructure.Data;

namespace SoftwareHamgueria.Infrastructure.Messaging;

/// <summary>
/// Adds the event to the request's DbContext without saving (ADR 0031): the repository's next SaveChanges commits the
/// order and its event together, or neither. The background dispatcher sends it later.
/// </summary>
public sealed class OutboxOrderEventPublisher(AppDbContext context) : IOrderEventPublisher
{
    public void Publish(string routingKey, string message) => context.OutboxMessages.Add(new OutboxMessage(routingKey, message));
}
