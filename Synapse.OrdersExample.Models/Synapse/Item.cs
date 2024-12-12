using Newtonsoft.Json;
using Synapse.OrdersExample.Models.Synapse;

namespace Synapse.OrdersExample.Models;

[Serializable]
public class Item {

    [JsonProperty("Status")]
    public Status Status { get; set; }

    [JsonProperty("Description")]
    public string? Description { get; set; }
    
    [JsonProperty("deliveryNotification")]
    public int DeliveryNotification { get; set; }
}