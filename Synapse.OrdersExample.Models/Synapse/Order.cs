using Newtonsoft.Json;

namespace Synapse.OrdersExample.Models;

[Serializable]
public class Order {
    [JsonProperty("OrderId")]
    public int OrderID { get; set; }
    
    [JsonProperty("Items")]
    public IEnumerable<Item> Items { get; set; }
}