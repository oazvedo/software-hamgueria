using SoftwareHamgueria.Application.Orders;
using SoftwareHamgueria.Domain.Entities;

namespace SoftwareHamgueria.UnitTests.Domain;

public class OrderStatusMovesTests
{
    private static Order At(OrderStatus status)
    {
        var order = new Order(Guid.NewGuid(), "Ana");
        if (status == OrderStatus.Draft) order.UpdateStatus(OrderStatus.Draft);
        if (status == OrderStatus.Done) order.UpdateStatus(OrderStatus.Done);
        if (status == OrderStatus.Cancelled) order.Cancel();
        return order;
    }

    [Theory(DisplayName = "012/CA-04: transições permitidas do status")]
    [InlineData(OrderStatus.Created, OrderStatus.Done)]
    [InlineData(OrderStatus.Created, OrderStatus.Cancelled)]
    [InlineData(OrderStatus.Created, OrderStatus.Draft)]
    [InlineData(OrderStatus.Draft, OrderStatus.Created)]
    [InlineData(OrderStatus.Draft, OrderStatus.Done)]
    [InlineData(OrderStatus.Draft, OrderStatus.Cancelled)]
    public void AllowedMoves(OrderStatus from, OrderStatus to)
    {
        var order = At(from);
        order.UpdateStatus(to);
        Assert.Equal(to, order.Status);
    }

    [Theory(DisplayName = "012/CA-05 (falha): servido e cancelado são finais")]
    [InlineData(OrderStatus.Done, OrderStatus.Created)]
    [InlineData(OrderStatus.Done, OrderStatus.Cancelled)]
    [InlineData(OrderStatus.Done, OrderStatus.Draft)]
    [InlineData(OrderStatus.Cancelled, OrderStatus.Done)]
    [InlineData(OrderStatus.Cancelled, OrderStatus.Created)]
    [InlineData(OrderStatus.Cancelled, OrderStatus.Cancelled)]
    public void FinalStatuses_DoNotMove(OrderStatus from, OrderStatus to)
    {
        var order = At(from);
        Assert.False(order.CanMoveTo(to));
        Assert.Throws<InvalidOperationException>(() => order.UpdateStatus(to));
        Assert.Equal(from, order.Status);
    }

    [Fact(DisplayName = "012/RN-01: o token de dono confere pelo hash, sem guardar o token")]
    public void OwnerToken_MatchesOnlyItsHash()
    {
        var token = OrderOwnerToken.New();
        var hash = OrderOwnerToken.Hash(token);

        Assert.True(OrderOwnerToken.Matches(hash, token));
        Assert.False(OrderOwnerToken.Matches(hash, OrderOwnerToken.New()));
        Assert.False(OrderOwnerToken.Matches(null, token));
        Assert.False(OrderOwnerToken.Matches(hash, null));
        Assert.DoesNotContain(token, hash);
        Assert.True(token.Length >= 43);
    }
}
