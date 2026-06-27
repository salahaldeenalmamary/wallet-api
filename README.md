# Wallet API — ASP.NET Core + PostgreSQL

A full-featured digital wallet REST API.

## Tech Stack

- **ASP.NET Core 10** (Web API)
- **Entity Framework Core 10** + **Npgsql** (PostgreSQL)
- **Swashbuckle** (Swagger / OpenAPI)

## Features

| Feature | Endpoint |
|---|---|
| Create wallet | `POST /api/wallets` |
| Get wallet + balance | `GET /api/wallets/{id}` |
| Deposit funds | `POST /api/wallets/{id}/deposit` |
| Withdraw funds | `POST /api/wallets/{id}/withdraw` |
| Force-withdraw (allows overdraft) | `POST /api/wallets/{id}/force-withdraw` |
| Check withdrawability | `GET /api/wallets/{id}/can-withdraw?amount=X` |
| Recalculate balance from DB | `POST /api/wallets/{id}/refresh-balance` |
| List transactions (paged) | `GET /api/wallets/{id}/transactions` |
| List transfers (paged) | `GET /api/wallets/{id}/transfers` |
| Transfer between wallets | `POST /api/transfers` |
| Force transfer | `POST /api/transfers/force` |
| Safe transfer (no error on failure) | `POST /api/transfers/safe` |
| Confirm a pending transaction | `POST /api/transactions/{uuid}/confirm` |
| Revert a confirmed transaction | `POST /api/transactions/{uuid}/revert` |

## Getting Started

### Prerequisites

- .NET 10 SDK
- PostgreSQL 14+

### 1. Configure the connection string

Edit `src/WalletApi/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "Default": "Host=localhost;Port=5432;Database=wallet_db;Username=postgres;Password=postgres"
  }
}
```

### 2. Run the API

```bash
cd dotnet-wallet
dotnet run --project src/WalletApi
```

The app auto-applies migrations on startup in the **Development** environment.

Open Swagger UI at: **http://localhost:5000** (redirects to HTTPS automatically)

### 3. Apply migrations manually (optional)

```bash
cd dotnet-wallet
dotnet ef database update --project src/WalletApi
```

## Project Structure

```
dotnet-wallet/
├── WalletApi.sln
└── src/
    └── WalletApi/
        ├── Controllers/        # HTTP endpoints
        ├── Data/               # EF Core DbContext + Migrations
        ├── Domain/
        │   ├── Enums/          # TransactionType, TransferStatus
        │   └── Exceptions/     # AmountInvalid, BalanceIsEmpty, InsufficientFunds
        ├── DTOs/
        │   ├── Requests/       # Input models
        │   └── Responses/      # Output models
        ├── Entities/           # EF Core entity classes
        ├── Middleware/         # Global exception handler
        ├── Services/           # Business logic
        ├── Program.cs          # App entry point + DI
        └── appsettings.json
```

## Decimal Precision

Amounts are stored as **integer-shifted integers** :
- `DecimalPlaces = 2` means $10.50 → stored as `1050`
- The API response always includes both `balance` (raw integer) and `balanceFloat` (human-readable decimal)

## Concurrency Safety

All balance-modifying operations use:
- **Serializable** PostgreSQL transactions
- **`SELECT … FOR UPDATE`** row-level locking on wallet rows
- Transfer operations lock wallets in **ascending ID order** to prevent deadlocks
