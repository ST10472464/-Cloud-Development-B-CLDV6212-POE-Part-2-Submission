using System.Text.Json.Serialization;

namespace Cloud_Development_B_CLDV_6212__POE_Part_1
{
    /// <summary>
    /// Order payload accepted by the queue producer endpoint (POST /api/orders/queue).
    /// Serialized to JSON and pushed onto the "order-processing-queue" Azure Storage queue.
    /// </summary>
    public class Order
    {
        [JsonPropertyName("OrderId")]
        public string OrderId { get; set; } = default!;

        [JsonPropertyName("CustomerName")]
        public string CustomerName { get; set; } = default!;

        [JsonPropertyName("SelectedItemSKUs")]
        public List<string> SelectedItemSKUs { get; set; } = new();

        [JsonPropertyName("TotalPrice")]
        public double TotalPrice { get; set; }

        [JsonPropertyName("OrderTimestamp")]
        public DateTime OrderTimestamp { get; set; }
    }
}