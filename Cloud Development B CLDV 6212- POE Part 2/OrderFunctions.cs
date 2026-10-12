using Azure.Storage.Queues;
using Cloud_Development_B_CLDV_6212__POE_Part_1;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text;
using System.Text.Json;

namespace CoffeeNChill.Functions
{
    // Team Member 1 - Keyur Keshav
    // Azure Storage Queue Producer & Order Validation
    public class OrderFunctions
    {
        private readonly ILogger<OrderFunctions> _logger;
        private readonly string _connectionString;
        private const string QueueName = "order-processing-queue";

        public OrderFunctions(ILogger<OrderFunctions> logger)
        {
            _logger = logger;
            _connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage") ?? "UseDevelopmentStorage=true";
        }

        /// <summary>
        /// Queue producer endpoint. Accepts an order, validates it, serializes it to the
        /// required JSON message payload and pushes it onto "order-processing-queue".
        /// Messages are Base64-encoded before being placed on the queue.
        /// </summary>
        [Function("QueueOrder")]
        public async Task<HttpResponseData> QueueOrder(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "orders/queue")] HttpRequestData req)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            Order? order;
            try
            {
                order = JsonSerializer.Deserialize<Order>(requestBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (JsonException)
            {
                _logger.LogWarning("Invalid JSON payload received for order queue request.");
                return await BadRequestAsync(req, "Invalid JSON payload in the request body.");
            }

            if (order == null || string.IsNullOrWhiteSpace(order.OrderId))
            {
                _logger.LogWarning("Order validation failed: OrderId is required.");
                return await BadRequestAsync(req, "Invalid order data. OrderId is required.");
            }

            if (string.IsNullOrWhiteSpace(order.CustomerName))
            {
                _logger.LogWarning("Order validation failed: CustomerName is required.");
                return await BadRequestAsync(req, "Invalid order data. CustomerName is required.");
            }

            if (order.SelectedItemSKUs == null || order.SelectedItemSKUs.Count == 0)
            {
                _logger.LogWarning("Order validation failed for {OrderId}: at least one item SKU is required.", order.OrderId);
                return await BadRequestAsync(req, "At least one item SKU is required.");
            }

            if (order.TotalPrice <= 0)
            {
                _logger.LogWarning("Order validation failed for {OrderId}: TotalPrice must be greater than zero.", order.OrderId);
                return await BadRequestAsync(req, "TotalPrice must be greater than zero.");
            }

            // Generate the order timestamp when the client did not supply one.
            if (order.OrderTimestamp == default)
            {
                order.OrderTimestamp = DateTime.UtcNow;
                _logger.LogInformation("OrderTimestamp generated for order {OrderId}.", order.OrderId);
            }

            try
            {
                var queueClient = new QueueClient(_connectionString, QueueName);
                await queueClient.CreateIfNotExistsAsync();

                // Serialize the order to the required JSON message payload structure.
                string message = JsonSerializer.Serialize(order);

                // Azure Storage queues store messages as Base64-encoded text.
                var bytes = Encoding.UTF8.GetBytes(message);
                var base64 = Convert.ToBase64String(bytes);

                await queueClient.SendMessageAsync(base64);

                _logger.LogInformation(
                    "Order {OrderId} for {CustomerName} (R{TotalPrice}) queued on {QueueName}.",
                    order.OrderId, order.CustomerName, order.TotalPrice, QueueName);

                var response = req.CreateResponse(HttpStatusCode.OK);
                await response.WriteAsJsonAsync(new { message = "Order queued successfully", orderId = order.OrderId });
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to queue order {OrderId}: queue connection or payload error.", order.OrderId);
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteStringAsync($"Failed to queue order: {ex.Message}");
                return errorResponse;
            }
        }

        private static async Task<HttpResponseData> BadRequestAsync(HttpRequestData req, string message)
        {
            var response = req.CreateResponse(HttpStatusCode.BadRequest);
            await response.WriteStringAsync(message);
            return response;
        }
    }
}