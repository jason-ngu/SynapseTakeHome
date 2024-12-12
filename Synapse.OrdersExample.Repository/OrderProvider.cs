using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Synapse.OrdersExample.Models;
using Synapse.OrdersExample.Providers.Interfaces;

namespace Synapse.OrdersExample.Providers;

/// <inheritdoc />
public class OrderProvider : IOrderProvider
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OrderProvider> _logger;
    private readonly ApiOptions _options;

    public OrderProvider(HttpClient httpClient, ILogger<OrderProvider> logger, IOptions<ApiOptions> options)
    {
        _httpClient = httpClient;
        _logger = logger;
        _options = options.Value;
    }

    public async Task<IEnumerable<Order>> FetchMedicalEquipmentOrders()
    {
        try
        {
            var response = await _httpClient.GetAsync(_options.Orders);
            response.EnsureSuccessStatusCode();
            var stringData = await response.Content.ReadAsStringAsync();
            IEnumerable<Order>? orders = JsonConvert.DeserializeObject<IEnumerable<Order>>(stringData);
            return orders;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to fetch orders from API: {ex.Message}");
        }
        return Enumerable.Empty<Order>();
    }

    public async Task SendItemDeliveryAlert(Item item, string orderId)
    {
        try
        {
            var alertData = new
            {
                Message = $"Alert for delivered item: Order {orderId}, Item: {item.Description}, " +
                          $"Delivery Notifications: {item.DeliveryNotification}"
            };
            var content = new StringContent(JsonConvert.SerializeObject(alertData), System.Text.Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(_options.Alert, content);
            response.EnsureSuccessStatusCode();
            _logger.LogInformation($"Alert sent for delivered item: {item.Description}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to send alert for delivered item: {item.Description}: {ex.Message}");
        }
    }

    public async Task UpdateOrder(Order order)
    {
        try
        {
            var content = new StringContent(JsonConvert.SerializeObject(order), System.Text.Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(_options.Update, content);
            response.EnsureSuccessStatusCode();
            _logger.LogInformation($"Updated order sent for processing: OrderId {order.OrderID}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to send updated order for processing: OrderId {order.OrderID}: {ex.Message}");
        }
    }
}