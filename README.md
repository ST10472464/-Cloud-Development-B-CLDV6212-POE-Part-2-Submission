# CoffeeNChill - Cloud Development B (CLDV6212) Part 2

## POE Part 2 Submission - Queue Triggers & Docker Compose Orchestration

---

### Team Members

| Name | Role |
|------|------|
| Keyur Keshav | Team Member 1 |
| Yadav Iserbelas | Team Member 2 |
| Saiyen Subban | Team Member 3 |

---

## Changelog

### Part 2 (Current)

- Added Azure Storage Queue producer endpoint `POST /api/orders/queue`
- Added `order-processing-queue` connection via Azurite
- Added `Order` model with the required JSON message payload structure
  (`OrderId`, `CustomerName`, `SelectedItemSKUs`, `TotalPrice`, `OrderTimestamp`)
- Added full client-side order validation with proper HTTP status codes (`400`, `500`)
- Messages are Base64-encoded before placement on the queue
- Added Postman collection with sample order request data in `/docs`
- *(Coming from other team members)* `ProcessOrderQueue` queue-triggered function,
  `Orders` Azure Table tracking, `docker-compose.yml` Docker Hub orchestration

---

## Part 2 Overview

During peak break times, hundreds of CoffeeNChill students place orders simultaneously. To prevent
synchronous processing from crashing the system, Part 2 introduces **asynchronous order queuing**:
orders are pushed onto an Azure Storage Queue and processed in the background.

This repository currently contains the **Team Member 1** deliverable: the Azure Storage Queue
producer and order validation logic.

---

## Order Queue Endpoint

| Method | Route                 | Description                          | Status Codes            |
| ------ | --------------------- | ------------------------------------ | ----------------------- |
| POST   | `/api/orders/queue`   | Queue an order for processing        | `200`, `400`, `500`     |

### Request Body (JSON)

```json
{
  "OrderId": "ORD-2026-8801",
  "CustomerName": "Jane Smith",
  "SelectedItemSKUs": ["COF-001", "PAS-104"],
  "TotalPrice": 65.00,
  "OrderTimestamp": "2026-10-12T10:15:00Z"
}
```

The endpoint validates that:

- `OrderId` is provided (required)
- `CustomerName` is provided (required)
- At least one item SKU exists in `SelectedItemSKUs`
- `TotalPrice` is greater than zero
- `OrderTimestamp` is generated server-side (`DateTime.UtcNow`) if omitted

On success the order is serialized to JSON, Base64-encoded, and pushed onto the
`order-processing-queue` storage queue. The response returns `200 OK` with
`{ "message": "Order queued successfully", "orderId": "..." }`.

### Example Response

```json
{
  "message": "Order queued successfully",
  "orderId": "ORD-2026-8801"
}
```

---

## Queue Details

| Item                | Value                       |
| ------------------- | --------------------------- |
| Queue name          | `order-processing-queue`    |
| Connection string   | `UseDevelopmentStorage=true` (Azurite) |
| Storage emulator    | Azurite (port 10002 for Queues) |
| Message format      | Base64-encoded JSON         |

---

## Local Setup

### 1. Prerequisites

- .NET 10 SDK
- Azure Functions Core Tools
- Docker Desktop
- Azurite (via Docker)

### 2. Run Azurite in an isolated container

```bash
docker run -d -p 10000:10000 -p 10001:10001 -p 10002:10002 --name azurite mcr.microsoft.com/azure-storage/azurite
```

Ports: `10000` Tables, `10001` Blobs, `10002` Queues.

### 3. local.settings.json

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated"
  }
}
```

### 4. Run the Functions locally

```bash
cd "Cloud Development B CLDV 6212- POE Part 1"
func start
```

---

## Testing with Postman

Import `/docs/CoffeeNChill Part 2 - Order Queue.postman_collection.json` and set the
collection variable:

```
{{baseUrl}} = http://localhost:7058
```

The collection includes:

- Queue Order with the sample payload
- Additional sample orders (including one without a timestamp)
- Validation failure cases (missing SKUs, zero price) that expect `400`
- Get All Orders to verify queue processing results

---

## Team Member Contributions

### Team Member 1 - Keyur Keshav (Azure Storage Queue Producer & Order Validation)

- Built the queue producer endpoint `POST /api/orders/queue`
- Serialized order data into the required JSON message payload structure
  (`OrderId`, `CustomerName`, `SelectedItemSKUs`, `TotalPrice`, `OrderTimestamp`)
- Established the connection to `order-processing-queue` in Azurite
- Added input validation (required fields, SKUs, positive price) with `400` responses
- Added robust error handling for queue connections and payload formatting (`500` + logging)
- Added timestamp generation when the client omits it
- Base64 encoding of queue messages
- Updated the Postman collection with sample request data and passing automated tests

### Team Member 2 - Yadav Iserbelas (Queue-Triggered Function & Order Lifecycle)

- `ProcessOrderQueue` function using `[QueueTrigger("order-processing-queue")]`
- `Orders` Azure Table tracking (PartitionKey: OrderDate, RowKey: OrderId)
- Lifecycle: Received → Preparing → Ready → Collected
- Poison queue handling

### Team Member 3 - Saiyen Subban (Docker Compose, Docker Hub & Documentation)

- `docker-compose.yml` orchestration with Docker Hub images (`image: username/app:v2.0`)
- Version-tagged image publishing (`v2.0`)
- README changelog and YouTube video link

---

## YouTube Demo

[Video Demo Link](#)

---

## Technology Stack

- .NET 10, Azure Functions v4 (Isolated Worker)
- Azure Storage Queues (via `Azure.Storage.Queues`)
- Azurite storage emulator
- Docker / Docker Compose

---

## References

- [Azure Functions .NET isolated worker](https://learn.microsoft.com/en-us/azure/azure-functions/dotnet-isolated-process-guide)
- [Azure Queue storage client library](https://learn.microsoft.com/en-us/azure/storage/queues/storage-quickstart-queues-dotnet)
- [Queue trigger bindings](https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-storage-queue-trigger)
- [Azurite emulator](https://learn.microsoft.com/en-us/azure/storage/common/storage-use-azurite)

---

## License

Educational use only - CLDV6212 POE