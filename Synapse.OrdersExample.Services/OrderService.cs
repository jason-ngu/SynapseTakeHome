using Microsoft.Extensions.Logging;
using Synapse.OrdersExample.Models;
using Synapse.OrdersExample.Providers.Interfaces;
using Synapse.OrdersExample.Services.Interfaces;

namespace Synapse.OrdersExample.Services;

/// <inheritdoc />
public class OrderService : IOrderService
{
    private readonly IOrderProvider _orderProvider;
    private readonly ILogger<OrderService> _logger;

    public OrderService(IOrderProvider orderProvider, ILogger<OrderService> logger) {
        _orderProvider = orderProvider;
        _logger = logger;
    }
    
    public async Task MonitorOrders() 
    {
        _logger.LogInformation("Start of App");

        IEnumerable<Order> medicalEquipmentOrders = await _orderProvider.FetchMedicalEquipmentOrders();
        foreach (Order order in medicalEquipmentOrders)
        {
            Order updatedOrder = ProcessOrder(order);
            await _orderProvider.UpdateOrder(updatedOrder);
        }

        _logger.LogInformation("Results sent to relevant APIs.");
    }
    
    /// <summary>
    /// Iterates through all items for a given order and updates each item if it is delivered
    /// </summary>
    /// <param name="order"></param>
    /// <returns>Order that has been updated with each item that is Delivered</returns>
    private Order ProcessOrder(Order order) 
    {
        IEnumerable<Item> items = order.Items;
        foreach(Item item in items) {
            if (IsItemDelivered(item)) {
                _orderProvider.SendItemDeliveryAlert(item, order.OrderID.ToString()).GetAwaiter().GetResult();
                IncrementDeliveryNotification(item);
            }
        }
        return order;
    }

    /// <summary>
    /// Returns a true if the item's status is Delivered, false otherwise
    /// </summary>
    /// <param name="item">The item that will be checked if it is delivered or not</param>
    /// <returns>Boolean value denoting if the item is delivered</returns>
    private bool IsItemDelivered(Item item) 
    {
        return item.Status == Models.Synapse.Status.Delivered;
    }

    /// <summary>
    /// Increments a given item's delivery notification
    /// </summary>
    /// <param name="item">The item that will have it's delivery notification incremented</param>
    private void IncrementDeliveryNotification(Item item) 
    {
        item.DeliveryNotification = item.DeliveryNotification + 1;
    }
}
