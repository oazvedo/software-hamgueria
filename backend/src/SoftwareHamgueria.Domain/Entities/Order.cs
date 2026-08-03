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

    public void UpdateStatus(OrderStatus status)
    {
        Status = status;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        Status = OrderStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }
}
