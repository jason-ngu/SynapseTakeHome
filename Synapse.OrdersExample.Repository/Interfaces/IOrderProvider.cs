using Synapse.OrdersExample.Models;

namespace Synapse.OrdersExample.Providers.Interfaces;

/// <summary>
/// Provider that will perform API calls related to Orders
/// </summary>
public interface IOrderProvider {
    /// <summary>
    /// Calls the Orders API to return a list of Medical Equipment Orders
    /// </summary>
    /// <returns>A list of orders from the API</returns>
    public Task<IEnumerable<Order>> FetchMedicalEquipmentOrders();

    /// <summary>
    /// Sends an alert to the Alert API for a given Item and OrderId
    /// </summary>
    /// <param name="item">The Item that needs to be alerted</param>
    /// <param name="orderId">The Order Id that corresponds to the Item to be alerted</param>
    /// <returns>An awaitable task</returns>
    public Task SendItemDeliveryAlert(Item item, string orderId);

    /// <summary>
    /// Calls the Update API to update a given Order
    /// </summary>
    /// <param name="orderToUpdate">The Order that will be updated</param>
    /// <returns>An awaitable task</returns>
    public Task UpdateOrder(Order orderToUpdate);
}
