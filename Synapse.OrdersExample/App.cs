using Synapse.OrdersExample.Services.Interfaces;

namespace Synapse.OrdersExample;

public class App
{
    private readonly IOrderService _orderService;

    public App(IOrderService orderService) 
    {
        _orderService = orderService;
    }

    /// <summary>
    /// Start monitoring orders
    /// </summary>
    public void Run() 
    {
        _orderService.MonitorOrders().GetAwaiter().GetResult();
    }
}
