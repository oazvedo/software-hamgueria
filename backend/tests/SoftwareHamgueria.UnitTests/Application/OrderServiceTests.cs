using SoftwareHamgueria.Application.Orders;
using FluentValidation;
using NSubstitute;
using SoftwareHamgueria.Application.DTOs.Order;
using SoftwareHamgueria.Application.Exceptions;
using SoftwareHamgueria.Application.Messaging;
using SoftwareHamgueria.Application.Repositories;
using SoftwareHamgueria.Application.Services;
using SoftwareHamgueria.Application.Validators.Order;
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
    private readonly Dish _dish = new("Burger Monolito", "Resolve tudo sozinho.", 34.90m, Category.Hamburgers);

    public OrderServiceTests()
    {
        _sut = new OrderService(_orders, _dishes, _tables, _comandas, _publisher,
            new CreateOrderDtoValidator(), new UpdateOrderDtoValidator(), new CloseComandaDtoValidator(),
            new ComandaQueryValidator());
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
    public async Task CreateAsync_ValidRequest_StagesEventThenSaves()
    {
        var result = await _sut.CreateAsync(Request());

        Assert.Equal(OrderStatus.Created, result.Status);
        Assert.Equal("Ana", result.CustomerName);
        Assert.False(result.ComandaClosed);
        // Outbox (ADR 0031): the event is staged before the write that commits both.
        Received.InOrder(() =>
        {
            _publisher.Publish("order.created", result.Id.ToString());
            _orders.AddAsync(Arg.Any<Order>());
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
        var ex = await Assert.ThrowsAsync<ValidationException>(() => _sut.CreateAsync(Request(customer)));

        Assert.Equal("CustomerName", Assert.Single(ex.Errors).PropertyName);

        await AssertNothingSavedOrPublished();
    }

    [Fact(DisplayName = "001/CA-05: pedido com prato inativo é recusado com o nome do prato")]
    public async Task CreateAsync_InactiveDish_IsRejected()
    {
        _dish.Disable();

        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() => _sut.CreateAsync(Request()));

        Assert.Equal("O prato Burger Monolito não está disponível.", ex.Message);
        await AssertNothingSavedOrPublished();
    }

    [Fact(DisplayName = "001/CA-05: pedido com prato inexistente é recusado")]
    public async Task CreateAsync_UnknownDish_IsRejected()
    {
        var ex = await Assert.ThrowsAsync<NotFoundException>(() => _sut.CreateAsync(Request(dishId: Guid.NewGuid())));

        Assert.Equal("O prato informado não existe.", ex.Message);

        await AssertNothingSavedOrPublished();
    }

    [Fact(DisplayName = "001/RN-03: pedido para mesa inexistente é recusado")]
    public async Task CreateAsync_UnknownTable_IsRejected()
    {
        var ex = await Assert.ThrowsAsync<NotFoundException>(() => _sut.CreateAsync(Request(tableId: Guid.NewGuid())));

        Assert.Equal("A mesa informada não existe.", ex.Message);

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
        await Assert.ThrowsAsync<ValidationException>(() => _sut.CloseComandaAsync(new CloseComandaDto(_table.Id, " ")));

        await _comandas.DidNotReceive().UpsertAsync(Arg.Any<Guid>(), Arg.Any<string>());
    }

    [Fact(DisplayName = "003/CA-02: pedido sem itens é recusado antes de ler o banco, sem gravar nem publicar")]
    public async Task CreateAsync_WithoutItems_IsRejectedBeforeReading()
    {
        await Assert.ThrowsAsync<ValidationException>(() => _sut.CreateAsync(new CreateOrderDto(_table.Id, "Ana", [])));

        await _tables.DidNotReceive().GetByIdAsync(Arg.Any<Guid>());
        await AssertNothingSavedOrPublished();
    }

    [Fact(DisplayName = "003/CA-03: alterar pedido com quantidade 0 é recusado sem gravar")]
    public async Task UpdateAsync_QuantityZero_IsRejected()
    {
        await Assert.ThrowsAsync<ValidationException>(() =>
            _sut.UpdateAsync(Guid.NewGuid(), new UpdateOrderDto([new OrderItemDto(_dish.Id, 0)]), OrderActor.Kitchen));

        await _orders.DidNotReceive().UpdateAsync(Arg.Any<Order>());
        _publisher.DidNotReceive().Publish(Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact(DisplayName = "003/CA-09: alterar pedido inexistente devolve não encontrado em português")]
    public async Task UpdateAsync_UnknownOrder_IsNotFound()
    {
        _orders.GetByIdAsync(Arg.Any<Guid>()).Returns((Order?)null);

        var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.UpdateAsync(Guid.NewGuid(), new UpdateOrderDto([new OrderItemDto(_dish.Id, 1)]), OrderActor.Kitchen));

        Assert.Equal("Pedido não encontrado.", ex.Message);
    }

    [Fact(DisplayName = "003/CA-05: consultar comanda sem nome é recusado")]
    public async Task GetComandaAsync_WithoutCustomer_IsRejected()
    {
        await Assert.ThrowsAsync<ValidationException>(() => _sut.GetComandaAsync(_table.Id, " "));
    }

    [Fact(DisplayName = "003/CA-05: fechar comanda grava o nome sem espaços nas pontas")]
    public async Task CloseComandaAsync_TrimsName()
    {
        await _sut.CloseComandaAsync(new CloseComandaDto(_table.Id, "  Ana  "));

        await _comandas.Received().UpsertAsync(_table.Id, "Ana");
    }
}
