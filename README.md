# payOS .NET Core Demo

This is a demo application showcasing the integration of the payOS .NET SDK for payment processing and payouts. It demonstrates how to use the payOS API to handle payment requests, manage payouts, and process webhooks.

## Features

- **Payment Requests**: Create, retrieve, and cancel payment links via the Order Controller
- **Payouts**: Handle single and batch transfers via the Transfer Controller
- **Webhooks**: Process payment confirmation webhooks via the Webhook Controller
- **Swagger UI**: Interactive API documentation and testing interface
- **Invoice Management**: Generate and download invoices for payments

## Prerequisites

- .NET 9.0 or later
- A payOS account with API credentials

## Installation

1. Clone the repository:

   ```pwsh
   git clone https://github.com/payOSHQ/payos-demo-dotnet-core.git
   cd payos-demo-dotnet-core
   ```

2. Restore dependencies:

   ```pwsh
   dotnet restore .\PayOSDemo.csproj
   ```

## Configuration

Before running the application, you need to configure your payOS credentials in [`appsettings.json`](./appsettings.json):

```json
{
  "PayOS": {
    "ClientId": "YOUR_CLIENT_ID",
    "ApiKey": "YOUR_API_KEY",
    "ChecksumKey": "YOUR_CHECKSUM_KEY",
    "PayoutClientId": "YOUR_PAYOUT_CLIENT_ID",
    "PayoutApiKey": "YOUR_PAYOUT_API_KEY",
    "PayoutChecksumKey": "YOUR_PAYOUT_CHECKSUM_KEY"
  }
}
```

Replace the placeholder values with your actual payOS credentials. You can obtain these from [payOS dashboard](https://my.payos.vn).

## Running the Application

1. Run the application:

   ```pwsh
   dotnet run
   ```

2. The application will start on `https://localhost:7028` (HTTPS) or `http://localhost:5022` (HTTP).

3. Access the Swagger UI for API testing `/swagger/index.html`.

## API Endpoints

### Order Controller (Payment Requests)

- `GET /api/order/{id}` - Get payment details by order ID
- `POST /api/order` - Create a new payment request
- `POST /api/order/{orderId}/cancel` - Cancel a payment request
- `GET /api/order/{orderId}/invoices` - Get invoices for an order
- `GET /api/order/{orderId}/invoices/{invoiceId}/download` - Download an invoice PDF

### Transfer Controller (Payouts)

- `GET /api/transfer/{id}` - Get transfer details by ID
- `GET /api/transfer` - List transfers with optional filters
- `POST /api/transfer` - Create a single transfer
- `POST /api/transfer/batch` - Create batch transfers
- `GET /api/transfer/account-balance` - Get account balance
- `POST /api/transfer/estimate-credit` - Estimate transfer fees

### Webhook Controller

- `POST /api/webhook/payment` - Process payment webhooks

## Usage Examples

### Creating a Payment Request

```pwsh
curl -X POST "https://localhost:7028/api/order" \
  -H "Content-Type: application/json" \
  -d '{
    "totalAmount": 100000,
    "description": "Test payment",
    "buyerName": "John Doe",
    "buyerEmail": "john@example.com",
    "items": [
      {
        "name": "Product A",
        "quantity": 1,
        "price": 100000
      }
    ]
  }'
```

### Creating a Payout

```pwsh
curl -X POST "https://localhost:7028/api/transfer" \
  -H "Content-Type: application/json" \
  -d '{
    "amount": 50000,
    "description": "Payout test",
    "toBin": "970422",
    "toAccountNumber": "1234567890"
  }'
```

## References

- [payOS .NET SDK](https://github.com/payOSHQ/payos-lib-dotnet) - The payOS .NET library
- [payOS API Documentation](https://payos.vn/docs/api/) - Complete API reference
