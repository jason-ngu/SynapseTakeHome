using Moq;
using Synapse.OrdersExample.Models;
using Synapse.OrdersExample.Services.Interfaces;
using Synapse.OrdersExample.Models.Synapse;
using Synapse.OrdersExample.Providers.Interfaces;
using Synapse.OrdersExample.Services;
using Microsoft.Extensions.Logging.Testing;

namespace Synapse.OrdersExample.UnitTests.Services;

public class OrderServiceTests
{
    private readonly Mock<IOrderProvider> _mockOrderProvider;
    private readonly FakeLogger<OrderService> _fakeLogger;
    private readonly IOrderService orderService;

    public OrderServiceTests()
    {
        _mockOrderProvider = new Mock<IOrderProvider>();
        _fakeLogger = new FakeLogger<OrderService>();
        orderService = new OrderService(_mockOrderProvider.Object, _fakeLogger);
    }

    [Fact]
    public void Run_ShouldProcessOrdersAndSendAlertsForDeliveredItems()
    {
        // Arrange
        var orders = new List<Order>
        {
            new Order
            {
                OrderID = 1,
                Items = new List<Item>
                {
                    new Item { Description = "Item 1", Status = Status.Delivered, DeliveryNotification = 0 }
                }
            },
            new Order
            {
                OrderID = 2,
                Items = new List<Item>
                {
                    new Item { Description = "Item 2", Status = Status.Delivered, DeliveryNotification = 0 }
                }
            }
        };

        _mockOrderProvider.Setup(s => s.FetchMedicalEquipmentOrders()).ReturnsAsync(orders);
        _mockOrderProvider.Setup(s => s.UpdateOrder(It.IsAny<Order>())).Returns(Task.CompletedTask);
        _mockOrderProvider.Setup(s => s.SendItemDeliveryAlert(It.IsAny<Item>(), It.IsAny<string>())).Returns(Task.CompletedTask);

        // Act
        orderService.MonitorOrders();

        // Assert that SendItemDeliveryAlert was called twice (once for each delivered item)
        _mockOrderProvider.Verify(s => s.SendItemDeliveryAlert(It.IsAny<Item>(), It.IsAny<string>()), Times.Exactly(orders.SelectMany(order => order.Items).Count(item => item.Status == Status.Delivered)));

        // Assert that UpdateOrder was called twice (once for each order)
        _mockOrderProvider.Verify(s => s.UpdateOrder(It.IsAny<Order>()), Times.Exactly(orders.Count));

        // Assert that the delivery notification count for delivered items was incremented
        Assert.Equal(1, orders[0].Items.First(i => i.Description == "Item 1").DeliveryNotification);
        Assert.Equal(1, orders[1].Items.First(i => i.Description == "Item 2").DeliveryNotification);
    }

    [Fact]
    public void Run_ShouldProcessOrdersAndNotSendAlertsForNonDeliveredItems()
    {
        // Arrange
        var orders = new List<Order>
        {
            new Order
            {
                OrderID = 1,
                Items = new List<Item>
                {
                    new Item { Description = "Item 1", Status = Status.Shipped, DeliveryNotification = 0 }
                }
            },
            new Order
            {
                OrderID = 2,
                Items = new List<Item>
                {
                    new Item { Description = "Item 2", Status = Status.Processing, DeliveryNotification = 0 }
                }
            }
        };

        _mockOrderProvider.Setup(s => s.FetchMedicalEquipmentOrders()).ReturnsAsync(orders);
        _mockOrderProvider.Setup(s => s.UpdateOrder(It.IsAny<Order>())).Returns(Task.CompletedTask);
        _mockOrderProvider.Setup(s => s.SendItemDeliveryAlert(It.IsAny<Item>(), It.IsAny<string>())).Returns(Task.CompletedTask);

        // Act
        orderService.MonitorOrders();

        // Assert that SendItemDeliveryAlert was not called since there are no delivered items
        _mockOrderProvider.Verify(s => s.SendItemDeliveryAlert(It.IsAny<Item>(), It.IsAny<string>()), Times.Exactly(0));

        // Assert that UpdateOrder was called twice (once for each order)
        _mockOrderProvider.Verify(s => s.UpdateOrder(It.IsAny<Order>()), Times.Exactly(orders.Count));

        // Assert that the delivery notification count for non delivered items was not incremented
        Assert.Equal(0, orders[0].Items.First(i => i.Description == "Item 1").DeliveryNotification);
        Assert.Equal(0, orders[1].Items.First(i => i.Description == "Item 2").DeliveryNotification);
    }
}
