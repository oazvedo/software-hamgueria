namespace SoftwareHamgueria.Domain.Entities;

public class Order
{
    public Guid Id { get; private set; }
    public Guid TableId { get; private set; }
    public Table Table { get; private set; } = null!;
    // Free-text tab owner; lets a table split its orders into separate comandas.
    public string CustomerName { get; private set; } = string.Empty;
    public List<OrderItem> Items { get; private set; } = new();
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public OrderStatus Status { get; private set; }
    /// <summary>SHA-256 of the owner token handed to whoever created the order (ADR 0030). Null on older orders.</summary>
    public string? OwnerTokenHash { get; private set; }

    /// <summary>
    /// Allowed status moves (spec 012). Served and cancelled are final; editing items sends the order back to Draft.
    /// </summary>
    private static readonly Dictionary<OrderStatus, OrderStatus[]> Moves = new()
    {
        [OrderStatus.Created] = [OrderStatus.Draft, OrderStatus.Done, OrderStatus.Cancelled],
        [OrderStatus.Draft] = [OrderStatus.Draft, OrderStatus.Created, OrderStatus.Done, OrderStatus.Cancelled],
        [OrderStatus.Done] = [],
        [OrderStatus.Cancelled] = [],
    };

    public bool CanMoveTo(OrderStatus next) => Moves.TryGetValue(Status, out var allowed) && allowed.Contains(next);

    public bool IsFinal => Status is OrderStatus.Done or OrderStatus.Cancelled;

    protected Order() { }

    public Order(Guid tableId, string customerName)
    {
        Id = Guid.NewGuid();
        TableId = tableId;
        CustomerName = customerName;
        Status = OrderStatus.Created;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddItem(Guid dishId, int quantity, string notes = "")
    {
        var existing = Items.FirstOrDefault(i => i.DishId == dishId);
        if (existing is not null)
        {
            Items.Remove(existing);
            var mergedNotes = string.IsNullOrWhiteSpace(notes) ? existing.Notes : notes;
            Items.Add(new OrderItem(Id, dishId, existing.Quantity + quantity, mergedNotes));
        }
        else
        {
            Items.Add(new OrderItem(Id, dishId, quantity, notes));
        }
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetOwnerTokenHash(string hash) => OwnerTokenHash = hash;

    public void UpdateStatus(OrderStatus status)
    {
        if (!CanMoveTo(status)) throw new InvalidOperationException($"Order {Id}: {Status} cannot move to {status}.");
        Status = status;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (!CanMoveTo(OrderStatus.Cancelled)) throw new InvalidOperationException($"Order {Id}: {Status} cannot be cancelled.");
        Status = OrderStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }
}
