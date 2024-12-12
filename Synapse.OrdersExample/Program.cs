using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Synapse.OrdersExample.Providers;
using Synapse.OrdersExample.Providers.Interfaces;
using Synapse.OrdersExample.Services;
using Synapse.OrdersExample.Services.Interfaces;

namespace Synapse.OrdersExample;

class Program {
    public static void Main(string[] args) {
        // Initialize host and set up dependency injections
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration(config => {
                config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                config.AddEnvironmentVariables().Build();
            })
            .ConfigureServices((hostContext, services) =>
            {
                services.Configure<ApiOptions>(hostContext.Configuration.GetSection(ApiOptions.Api));

                services.AddHttpClient<IOrderProvider, OrderProvider>();

                services.AddScoped<IOrderService, OrderService>();

                services.AddScoped<IOrderProvider, OrderProvider>();

                services.AddScoped<App>();
            })
            .Build();
        var app = host.Services.GetRequiredService<App>();
        app.Run();
    }
}