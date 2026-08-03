namespace SoftwareHamgueria.Domain.Entities;

public class OrderItem
{
    public Guid Id { get; private set; }
    public Guid OrderId { get; private set; }
    public Guid DishId { get; private set; }
    public int Quantity { get; private set; }
    // Per-item note for the kitchen (e.g. "sem cebola").
    public string Notes { get; private set; } = string.Empty;
    public Dish Dish { get; private set; } = null!;

    protected OrderItem() { }

    public OrderItem(Guid orderId, Guid dishId, int quantity, string notes = "")
    {
        Id = Guid.NewGuid();
        OrderId = orderId;
        DishId = dishId;
        Quantity = quantity;
        Notes = notes;
    }
}
