using SoftwareHamgueria.Domain.Entities;

namespace SoftwareHamgueria.UnitTests.Domain;

public class OrderTests
{
    private static readonly Guid TableId = Guid.NewGuid();
    private static readonly Guid DishId = Guid.NewGuid();

    [Fact(DisplayName = "001/RN-05: pedido novo nasce como recebido (Created)")]
    public void Constructor_NewOrder_StartsAsCreated()
    {
        var order = new Order(TableId, "Ana");

        Assert.Equal(OrderStatus.Created, order.Status);
        Assert.Empty(order.Items);
    }

    [Fact(DisplayName = "001/RN-04: o mesmo prato pedido duas vezes vira um item com as quantidades somadas")]
    public void AddItem_SameDishTwice_MergesQuantities()
    {
        var order = new Order(TableId, "Ana");

        order.AddItem(DishId, 1);
        order.AddItem(DishId, 2);

        var item = Assert.Single(order.Items);
        Assert.Equal(3, item.Quantity);
    }

    [Fact(DisplayName = "001/RN-04: observação nova substitui a anterior; observação vazia mantém a anterior")]
    public void AddItem_SameDishWithNotes_KeepsLatestNonEmptyNote()
    {
        var order = new Order(TableId, "Ana");

        order.AddItem(DishId, 1, "sem cebola");
        order.AddItem(DishId, 1);
        Assert.Equal("sem cebola", Assert.Single(order.Items).Notes);

        order.AddItem(DishId, 1, "bem passado");
        Assert.Equal("bem passado", Assert.Single(order.Items).Notes);
    }

    [Fact(DisplayName = "001/RN-04: pratos diferentes viram itens separados")]
    public void AddItem_DifferentDishes_CreatesSeparateItems()
    {
        var order = new Order(TableId, "Ana");

        order.AddItem(DishId, 1);
        order.AddItem(Guid.NewGuid(), 1);

        Assert.Equal(2, order.Items.Count);
    }

    [Fact(DisplayName = "001/RN-05: cancelar muda o status para cancelado")]
    public void Cancel_CreatedOrder_BecomesCancelled()
    {
        var order = new Order(TableId, "Ana");

        order.Cancel();

        Assert.Equal(OrderStatus.Cancelled, order.Status);
    }
}
