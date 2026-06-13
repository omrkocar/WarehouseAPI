using WarehouseAPI.Models.Entities;

namespace WarehouseAPI.Tests;
using Xunit;

public class OrderStateMachineTests
{
    [Theory]
    [InlineData(OrderStatus.Pending, OrderStatus.Paid, true)]
    [InlineData(OrderStatus.Pending, OrderStatus.Cancelled, true)]
    [InlineData(OrderStatus.Pending, OrderStatus.Shipped, false)]
    [InlineData(OrderStatus.Paid, OrderStatus.Shipped, true)]
    [InlineData(OrderStatus.Paid, OrderStatus.Cancelled, true)]
    [InlineData(OrderStatus.Paid, OrderStatus.Pending, false)]
    [InlineData(OrderStatus.Shipped, OrderStatus.Cancelled, false)]
    [InlineData(OrderStatus.Shipped, OrderStatus.Paid, false)]
    [InlineData(OrderStatus.Cancelled, OrderStatus.Pending, false)]
    [InlineData(OrderStatus.Cancelled, OrderStatus.Paid, false)]
    public void CanTransitionTo_ReturnsExpectedResult(
        OrderStatus from, OrderStatus to, bool expected)
    {
        var order = new Order
        {
            CustomerName = "Test",
            Status = from,
            CreatedAt = DateTime.UtcNow,
            Items = new List<OrderItem>()
        };

        var result = order.CanTransitionTo(to);

        Assert.Equal(expected, result);
    }
}