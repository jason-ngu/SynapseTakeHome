using Moq;
using System.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Synapse.OrdersExample.Models;
using Synapse.OrdersExample.Providers;
using Newtonsoft.Json;
using Moq.Protected;
using Microsoft.Extensions.Logging.Testing;

namespace Synapse.OrdersExample.UnitTests.Providers;

public class OrderProviderTests
{
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private readonly FakeLogger<OrderProvider> _fakeLogger;
    private readonly Mock<IOptions<ApiOptions>> _mockOptions;
    private readonly ApiOptions _apiOptions;
    private readonly OrderProvider orderProvider;

    public OrderProviderTests()
    {
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        var httpClient = new HttpClient(_mockHttpMessageHandler.Object);
        _fakeLogger = new FakeLogger<OrderProvider>();
        _apiOptions = new ApiOptions
        {
            Orders = "https://api.example.com/orders",
            Alert = "https://api.example.com/alert",
            Update = "https://api.example.com/update"
        };
        _mockOptions = new Mock<IOptions<ApiOptions>>();
        _mockOptions.Setup(x => x.Value).Returns(_apiOptions);
        orderProvider = new OrderProvider(httpClient, _fakeLogger, _mockOptions.Object);
    }

    [Fact]
    public async Task FetchMedicalEquipmentOrders_ShouldReturnOrders_WhenApiCallIsSuccessful()
    {
        // Arrange
        var orders = new List<Order>
        {
            new Order { OrderID = 1, Items = Enumerable.Empty<Item>() },
            new Order { OrderID = 2, Items = Enumerable.Empty<Item>() }
        };

        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonConvert.SerializeObject(orders))
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(response);

        // Act
        var result = await orderProvider.FetchMedicalEquipmentOrders();

        // Assert
        Assert.NotEmpty(result);
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task FetchMedicalEquipmentOrders_ShouldReturnEmpty_WhenApiCallFails()
    {
        // Arrange
        var response = new HttpResponseMessage(HttpStatusCode.InternalServerError);

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(response);

        // Act
        var result = await orderProvider.FetchMedicalEquipmentOrders();

        // Assert
        Assert.Empty(result);
        Assert.NotNull(_fakeLogger.Collector.LatestRecord);
        Assert.Contains("Failed to fetch orders from API:", _fakeLogger.Collector.LatestRecord.Message);
        Assert.Equal(LogLevel.Error, _fakeLogger.Collector.LatestRecord.Level);
    }

    [Fact]
    public async Task SendItemDeliveryAlert_ShouldSendAlert_WhenApiCallIsSuccessful()
    {
        // Arrange
        var item = new Item { Description = "Item A", DeliveryNotification = 0 };
        var orderId = "1";

        var response = new HttpResponseMessage(HttpStatusCode.OK);

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(response);

        // Act
        await orderProvider.SendItemDeliveryAlert(item, orderId);

        // Assert
        _mockHttpMessageHandler
            .Protected()
            .Verify<Task<HttpResponseMessage>>("SendAsync",
            Times.Exactly(1),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.Method == HttpMethod.Post
                && req.RequestUri.ToString() == _apiOptions.Alert
            ),
        ItExpr.IsAny<CancellationToken>());
        Assert.Contains("Alert sent for delivered item: Item A", _fakeLogger.Collector.LatestRecord.Message);
    }

    [Fact]
    public async Task SendItemDeliveryAlert_ShouldLogError_WhenApiCallFails()
    {
        // Arrange
        var item = new Item { Description = "Item B", DeliveryNotification = 0 };
        var orderId = "124";

        var response = new HttpResponseMessage(HttpStatusCode.BadRequest);

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(response);

        // Act
        await orderProvider.SendItemDeliveryAlert(item, orderId);

        // Assert
        Assert.NotNull(_fakeLogger.Collector.LatestRecord);
        Assert.Contains("Failed to send alert for delivered item: Item B", _fakeLogger.Collector.LatestRecord.Message);
        Assert.Equal(LogLevel.Error, _fakeLogger.Collector.LatestRecord.Level);
    }

    [Fact]
    public async Task UpdateOrder_ShouldSendUpdatedOrder_WhenApiCallIsSuccessful()
    {
        // Arrange
        var order = new Order { OrderID = 1, Items = Enumerable.Empty<Item>() };

        var response = new HttpResponseMessage(HttpStatusCode.OK);

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(response);

        // Act
        await orderProvider.UpdateOrder(order);

        // Assert
        _mockHttpMessageHandler
            .Protected()
            .Verify<Task<HttpResponseMessage>>("SendAsync",
            Times.Exactly(1),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.Method == HttpMethod.Post
                && req.RequestUri.ToString() == _apiOptions.Update
            ),
        ItExpr.IsAny<CancellationToken>());
        Assert.Contains("Updated order sent for processing: OrderId 1", _fakeLogger.Collector.LatestRecord.Message);
    }

    [Fact]
    public async Task UpdateOrder_ShouldLogError_WhenApiCallFails()
    {
        // Arrange
        var order = new Order { OrderID = 1, Items = Enumerable.Empty<Item>() };

        var response = new HttpResponseMessage(HttpStatusCode.BadRequest);

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(response);

        // Act
        await orderProvider.UpdateOrder(order);

        // Assert
        Assert.NotNull(_fakeLogger.Collector.LatestRecord);
        Assert.Contains("Failed to send updated order for processing: OrderId 1", _fakeLogger.Collector.LatestRecord.Message);
        Assert.Equal(LogLevel.Error, _fakeLogger.Collector.LatestRecord.Level);
    }
}
