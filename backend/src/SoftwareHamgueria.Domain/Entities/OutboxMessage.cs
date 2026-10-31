namespace SoftwareHamgueria.Domain.Entities;

/// <summary>
/// An event waiting to be sent to the broker, written in the same transaction as the change that produced it
/// (transactional outbox, ADR 0031). The dispatcher marks it sent after the broker confirms.
/// </summary>
public class OutboxMessage
{
    public Guid Id { get; private set; }
    public string RoutingKey { get; private set; } = string.Empty;
    public string Payload { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public DateTime? SentAt { get; private set; }
    public int Attempts { get; private set; }
    public string? LastError { get; private set; }

    protected OutboxMessage() { }

    public OutboxMessage(string routingKey, string payload)
    {
        Id = Guid.NewGuid();
        RoutingKey = routingKey;
        Payload = payload;
        CreatedAt = DateTime.UtcNow;
    }

    public void MarkSent() => SentAt = DateTime.UtcNow;

    public void MarkFailed(string error)
    {
        Attempts++;
        LastError = error.Length > 500 ? error[..500] : error;
    }
}
