# CoffeeNChill - Cloud Development B (CLDV6212) POE

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

### Part 1 (Included)

- Azure Table Storage emulated by **Azurite** for the `MenuItems` table
- Azure Blob Storage container `staff-documents` for operational files
- HTTP-triggered Azure Functions for Menu CRUD and staff Documents
- Input validation and proper HTTP status codes (`200`, `201`, `400`, `404`)
- Dockerfiles for the Functions app and Azurite

---

## Project Overview

CoffeeNChill is a cloud-enabled canteen management system for a campus environment. This
repository contains the **Part 1** foundation (Azure Functions, Tables & Files) and the
**Part 2** asynchronous order queue workflow, all running locally against the **Azurite**
storage emulator.

The repository contains **all Part 1 code** (Menu API, Document API) and **Team Member 1's
Part 2 code** (Azure Storage Queue producer with order validation).

---

## Part 1 - Menu & Documents APIs

### Menu Endpoints

| Method | Route                             | Description                                 | Status Codes        |
| ------ | --------------------------------- | ------------------------------------------- | ------------------- |
| POST   | `/api/menu`                       | Create a new menu item                      | `201`, `400`, `409` |
| GET    | `/api/menu`                       | Get all menu items                          | `200`               |
| GET    | `/api/menu/category/{category}`   | Get menu items filtered by category         | `200`               |
| PUT    | `/api/menu/{category}/{id}`       | Update a menu item (price / availability)   | `200`, `400`, `404` |
| DELETE | `/api/menu/{category}/{id}`       | Delete a menu item                          | `200`, `404`        |

**MenuItems table schema:** `PartitionKey` = Category (e.g. `Hot Drinks`), `RowKey` = SKU
(e.g. `COF-001`), plus `Name`, `Description`, `Price` (double), `IsAvailable` (bool).

### Document Endpoints

| Method | Route                              | Description                            | Status Codes |
| ------ | ---------------------------------- | -------------------------------------- | ------------ |
| POST   | `/api/documents/upload`            | Upload a staff document                | `201`, `400` |
| POST   | `/api/documents/upload/{fileName}` | Upload a binary (Base64) document      | `201`, `400` |
| GET    | `/api/documents`                   | List documents (name, size, modified)  | `200`        |
| GET    | `/api/documents/download/{fileName}` | Download a document as a stream      | `200`, `404` |

---

## Part 2 - Order Queue Producer

### Order Queue Endpoint

| Method | Route               | Description                   | Status Codes        |
| ------ | ------------------- | ----------------------------- | ------------------- |
| POST   | `/api/orders/queue` | Queue an order for processing | `200`, `400`, `500` |

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

Validation performed:

- `OrderId` required
- `CustomerName` required
- At least one SKU in `SelectedItemSKUs`
- `TotalPrice` greater than zero
- `OrderTimestamp` generated server-side (`DateTime.UtcNow`) if omitted

On success the order is serialized to JSON, Base64-encoded, and pushed onto
`order-processing-queue`. Response: `200 OK` with
`{ "message": "Order queued successfully", "orderId": "..." }`.

### Queue Details

| Item              | Value                        |
| ----------------- | ---------------------------- |
| Queue name        | `order-processing-queue`     |
| Connection string | `UseDevelopmentStorage=true` (Azurite) |
| Storage emulator  | Azurite (port 10002 for Queues) |
| Message format    | Base64-encoded JSON          |

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
cd "Cloud Development B CLDV 6212- POE Part 2"
func start
```

---

## Docker Execution

### Build the Functions image

```bash
docker build -t st10472501/coffeennchill-functions:v2.0 .
```

### Run the Functions container

```bash
docker run --network host -p 7071:80 st10472501/coffeennchill-functions:v2.0
```

### Push to Docker Hub

```bash
docker push st10472501/coffeennchill-functions:v2.0
```

---

## Testing with Postman

Both collections are in `/docs`. Set the collection variable:

```
{{baseUrl}} = http://localhost:7058
```

- `CoffeeNChill-Part1-Complete.postman_collection.json` - Menu and Document endpoints
- `CoffeeNChill Part 2 - Order Queue.postman_collection.json` - Order queue producer with
  sample payloads, validation failure cases, and verification requests

---

## Team Member Contributions

### Team Member 1 - Keyur Keshav (Tables & Menu CRUD / Queue Producer)

**Part 1**

- Set up the C# Azure Functions isolated project structure
- Configured `local.settings.json` for Azurite (`UseDevelopmentStorage=true`)
- Created the `MenuItem` entity model (`PartitionKey` = Category, `RowKey` = SKU)
- Implemented all Menu HTTP functions (Create, Get All, Get By Category, Update, Delete)
- Input validation and proper HTTP status codes (`400`, `404`, `409`)

**Part 2**

- Built the queue producer endpoint `POST /api/orders/queue`
- Serialized order data into the required JSON message payload structure
- Established the connection to `order-processing-queue` in Azurite
- Complete client payload validation with `400` responses
- Robust error handling for queue connections and payload formatting (`500` + logging)
- Server-side timestamp generation and Base64-encoded queue messages
- Updated the Postman collection with sample request data

### Team Member 2 - Yadav Iserbelas

- Part 1: Blob storage document functions (Upload, List, Download)
- Part 2: `ProcessOrderQueue` queue-triggered function, `Orders` table lifecycle
  (Received → Preparing → Ready → Collected), poison queue handling

### Team Member 3 - Saiyen Subban

- Part 1: Dockerfiles, README, video
- Part 2: `docker-compose.yml` orchestration with Docker Hub images
  (`image: <username>/coffeennchill-functions:v2.0`), version-tagged publishing, video

---

## Repository Structure

```
├── Cloud Development B CLDV 6212- POE Part 2/   # Azure Functions project (Part 1 + Part 2)
│   ├── MenuFunctions.cs       # Part 1 - Menu CRUD HTTP functions
│   ├── MenuItem.cs            # Part 1 - MenuItems table entity
│   ├── DocumentFunctions.cs   # Part 1 - Staff document functions
│   ├── OrderFunctions.cs      # Part 2 - Order queue producer
│   ├── Order.cs               # Part 2 - Order JSON payload model
│   ├── Program.cs / host.json / local.settings.json
│   └── Cloud Development B CLDV 6212- POE Part 2.csproj / .slnx
├── docs/                      # Postman collections + sample files
├── Dockerfile                 # Functions app image
└── Dockerfile.azurite         # Azurite image
```

---

## YouTube Demo

[Video Demo Link](#)

---

## Technology Stack

- .NET 10, Azure Functions v4 (Isolated Worker)
- Azure Table Storage (`Azure.Data.Tables`)
- Azure Blob Storage (`Azure.Storage.Blobs`)
- Azure Storage Queues (`Azure.Storage.Queues`)
- Azurite storage emulator
- Docker / Docker Compose

---

## References

- [Azure Functions .NET isolated worker](https://learn.microsoft.com/en-us/azure/azure-functions/dotnet-isolated-process-guide)
- [Azure Tables client library](https://learn.microsoft.com/en-us/azure/storage/tables/table-storage-how-to-use-dotnet)
- [Azure Blob storage client library](https://learn.microsoft.com/en-us/azure/storage/blobs/storage-quickstart-blobs-dotnet)
- [Azure Queue storage client library](https://learn.microsoft.com/en-us/azure/storage/queues/storage-quickstart-queues-dotnet)
- [Queue trigger bindings](https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-storage-queue-trigger)
- [Azurite emulator](https://learn.microsoft.com/en-us/azure/storage/common/storage-use-azurite)

---

## License

Educational use only - CLDV6212 POE