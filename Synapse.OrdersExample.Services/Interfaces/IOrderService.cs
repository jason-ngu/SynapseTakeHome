namespace Synapse.OrdersExample.Services.Interfaces;

/// <summary>
/// Order Service that has business logic
/// </summary>
public interface IOrderService
{
    /// <summary>
    /// Retrieve a list of orders from the API
    /// Process each order and if it is in the Delivered state, send a delivery alert and increment DeliveryNotification
    /// Update the order   
    /// </summary>
    Task MonitorOrders();
}
