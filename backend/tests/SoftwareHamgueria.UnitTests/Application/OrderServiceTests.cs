using NSubstitute;
using SoftwareHamgueria.Application.DTOs.Order;
using SoftwareHamgueria.Application.Messaging;
using SoftwareHamgueria.Application.Repositories;
using SoftwareHamgueria.Application.Services;
using SoftwareHamgueria.Domain.Entities;

namespace SoftwareHamgueria.UnitTests.Application;

public class OrderServiceTests
{
    private readonly IOrderRepository _orders = Substitute.For<IOrderRepository>();
    private readonly IDishRepository _dishes = Substitute.For<IDishRepository>();
    private readonly ITableRepository _tables = Substitute.For<ITableRepository>();
    private readonly IComandaRepository _comandas = Substitute.For<IComandaRepository>();
    private readonly IOrderEventPublisher _publisher = Substitute.For<IOrderEventPublisher>();
    private readonly OrderService _sut;

    private readonly Table _table = new("Mesa 01");
    private readonly Dish _dish = new("Burger Monolito", "Resolve tudo sozinho.", 34.90, Category.Hamburgers);

    public OrderServiceTests()
    {
        _sut = new OrderService(_orders, _dishes, _tables, _comandas, _publisher);
        _tables.GetByIdAsync(_table.Id).Returns(_table);
        _dishes.GetByIdAsync(_dish.Id).Returns(_dish);

        // The service re-reads the order after saving; hand back what was added.
        Order? saved = null;
        _orders.AddAsync(Arg.Do<Order>(o => saved = o)).Returns(Task.CompletedTask);
        _orders.GetByIdAsync(Arg.Any<Guid>()).Returns(_ => saved);
    }

    private CreateOrderDto Request(string customer = "Ana", Guid? dishId = null, Guid? tableId = null) =>
        new(tableId ?? _table.Id, customer, [new OrderItemDto(dishId ?? _dish.Id, 1)]);

    private async Task AssertNothingSavedOrPublished()
    {
        await _orders.DidNotReceive().AddAsync(Arg.Any<Order>());
        _publisher.DidNotReceive().Publish(Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact(DisplayName = "001/CA-03: pedido válido é gravado como recebido e publica order.created depois de gravar")]
    public async Task CreateAsync_ValidRequest_SavesThenPublishes()
    {
        var result = await _sut.CreateAsync(Request());

        Assert.Equal(OrderStatus.Created, result.Status);
        Assert.Equal("Ana", result.CustomerName);
        Assert.False(result.ComandaClosed);
        Received.InOrder(() =>
        {
            _orders.AddAsync(Arg.Any<Order>());
            _publisher.Publish("order.created", result.Id.ToString());
        });
    }

    [Fact(DisplayName = "001/RN-06: o nome da comanda é gravado sem espaços nas pontas")]
    public async Task CreateAsync_CustomerNameWithSpaces_IsTrimmed()
    {
        var result = await _sut.CreateAsync(Request(customer: "  Ana  "));

        Assert.Equal("Ana", result.CustomerName);
    }

    [Theory(DisplayName = "001/CA-04: pedido sem nome de comanda é recusado")]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreateAsync_WithoutCustomerName_IsRejected(string customer)
    {
        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.CreateAsync(Request(customer)));

        await AssertNothingSavedOrPublished();
    }

    [Fact(DisplayName = "001/CA-05: pedido com prato inativo é recusado com o nome do prato")]
    public async Task CreateAsync_InactiveDish_IsRejected()
    {
        _dish.Disable();

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.CreateAsync(Request()));

        Assert.Contains(_dish.Name, ex.Message);
        await AssertNothingSavedOrPublished();
    }

    [Fact(DisplayName = "001/CA-05: pedido com prato inexistente é recusado")]
    public async Task CreateAsync_UnknownDish_IsRejected()
    {
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.CreateAsync(Request(dishId: Guid.NewGuid())));

        await AssertNothingSavedOrPublished();
    }

    [Fact(DisplayName = "001/RN-03: pedido para mesa inexistente é recusado")]
    public async Task CreateAsync_UnknownTable_IsRejected()
    {
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.CreateAsync(Request(tableId: Guid.NewGuid())));

        await AssertNothingSavedOrPublished();
    }

    /// <summary>The comanda "Ana" with one order and one closure, as the service reports it.</summary>
    private async Task<OrderResponseDto> ComandaWith(Order order, ComandaClosure closure)
    {
        _orders.GetByComandaAsync(_table.Id, "Ana").Returns([order]);
        _comandas.GetAsync(_table.Id, "Ana").Returns(closure);
        return Assert.Single(await _sut.GetComandaAsync(_table.Id, "Ana"));
    }

    [Fact(DisplayName = "001/CA-08: pedido feito antes do fechamento pertence a uma comanda fechada")]
    public async Task GetComandaAsync_OrderBeforeClosure_IsClosed()
    {
        var order = new Order(_table.Id, "Ana");
        var closure = new ComandaClosure(_table.Id, "Ana");

        Assert.True((await ComandaWith(order, closure)).ComandaClosed);
    }

    [Fact(DisplayName = "001/CA-08: pedido novo com o mesmo nome depois do fechamento abre comanda nova")]
    public async Task GetComandaAsync_OrderAfterClosure_IsOpen()
    {
        var closure = new ComandaClosure(_table.Id, "Ana");
        // Entities stamp DateTime.UtcNow themselves, so wait for the next tick to get a later order.
        // Remove when time is injected (backlog: TimeProvider).
        SpinWait.SpinUntil(() => DateTime.UtcNow > closure.ClosedAt);
        var order = new Order(_table.Id, "Ana");

        Assert.False((await ComandaWith(order, closure)).ComandaClosed);
    }

    [Fact(DisplayName = "001/CA-08: fechar comanda sem nome é recusado")]
    public async Task CloseComandaAsync_WithoutCustomerName_IsRejected()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.CloseComandaAsync(_table.Id, " "));

        await _comandas.DidNotReceive().UpsertAsync(Arg.Any<Guid>(), Arg.Any<string>());
    }
}
