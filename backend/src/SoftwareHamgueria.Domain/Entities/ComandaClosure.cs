namespace SoftwareHamgueria.Domain.Entities;

// Records the last time a comanda (table + customer name) was closed. A comanda
// is considered OPEN again if it has orders created after this timestamp, which
// lets the same name be reused as a fresh tab later.
public class ComandaClosure
{
    public Guid Id { get; private set; }
    public Guid TableId { get; private set; }
    public string CustomerName { get; private set; } = string.Empty;
    public DateTime ClosedAt { get; private set; }

    protected ComandaClosure() { }

    public ComandaClosure(Guid tableId, string customerName)
    {
        Id = Guid.NewGuid();
        TableId = tableId;
        CustomerName = customerName;
        ClosedAt = DateTime.UtcNow;
    }

    public void Touch() => ClosedAt = DateTime.UtcNow;
}
