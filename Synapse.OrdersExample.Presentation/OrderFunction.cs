using Synapse.OrdersExample.Services.Interfaces;

namespace Synapse.OrdersExample.Presentation;

public class OrderFunction
{
    private readonly IOrderService _orderService;

    public OrderFunction(IOrderService orderService)
    {
        _orderService = orderService;
    }

    public async Task MonitorOrders() 
    {
        await _orderService.MonitorOrders();
    }
}
