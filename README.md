# PaySphere - Complete Documentation

A microservices-based digital wallet and payment platform built with .NET 8, featuring secure user authentication, wallet management, and peer-to-peer transfers.

## Overview

PaySphere is a modern, cloud-ready microservices architecture that demonstrates enterprise-grade patterns for financial transaction management. The system separates concerns across independent services: user authentication/authorization (AuthService) and wallet/transaction management (WalletService), fronted by an API Gateway (Ocelot) for unified client access.

Key principles:
- **Data ownership**: Each microservice owns its database
- **Loose coupling**: Service-to-service communication via HTTP
- **Atomic operations**: Database transactions ensure financial consistency
- **Auditability**: Immutable ledger of all transactions

## Architecture

```
┌─────────────┐
│   Client    │
└──────┬──────┘
	   │ HTTPS
	   ▼
┌──────────────────────────────────────┐
│    API Gateway (Ocelot)              │  Port 7265
│  - Route aggregation                 │
│  - Swagger UI aggregation            │
│  - Health checks                     │
└──────┬───────────────────┬───────────┘
	   │                   │
	   ▼                   ▼
┌─────────────────┐  ┌──────────────────┐
│  AuthService    │  │ WalletService    │  Ports 7213, 7261
│ - JWT issuance  │  │ - Wallet ops     │
│ - User mgmt     │  │ - Transfers      │
│ - Role RBAC     │  │ - Transaction    │
└────────┬────────┘  │   history        │
		 │           └────────┬─────────┘
		 │                    │
		 ▼                    ▼
	┌─────────┐          ┌──────────┐
	│AuthDb   │          │WalletDb  │  SQL Server
	└─────────┘          └──────────┘
```

## Technology Stack

| Layer | Technology |
|-------|-----------|
| **Language** | C# / .NET 8 |
| **Framework** | ASP.NET Core Minimal APIs |
| **Database** | SQL Server with EF Core Code-First |
| **Authentication** | JWT (HS256 symmetric) |
| **Gateway** | Ocelot (reverse proxy) |
| **ORM** | Entity Framework Core |
| **Logging** | Serilog (console + rolling files) |
| **Testing** | NUnit + Moq + FluentAssertions |
| **Testing DB** | SQLite in-memory |

## Microservices

### AuthService

**Responsibility**: User identity and authentication.

**Key endpoints**:
- `POST /api/v1/auth/register` — Create new user
- `POST /api/v1/auth/login` — Issue JWT
- `GET /api/v1/auth/profile` — Retrieve authenticated user info
- `PUT /api/v1/auth/change-password` — Update password
- `GET /api/v1/internal/users/{id}` — Internal user validation (used by WalletService)

**Entities**:
- `User` — FullName, Email, PasswordHash, PhoneNumber, RoleId, IsActive
- `Role` — Name, Description

**Key features**:
- Password hashing via `Microsoft.AspNetCore.Identity.PasswordHasher`
- JWT generation with configurable expiry
- Email/phone uniqueness enforcement (unique DB indexes)
- Default roles (Admin, User) seeded on startup

**Port**: `7213`

### WalletService

**Responsibility**: Wallet management and financial transactions.

**Key endpoints**:
- `POST /api/v1/wallets` — Create wallet
- `GET /api/v1/wallets` — Get wallet info
- `GET /api/v1/wallets/balance` — Get current balance
- `POST /api/v1/wallets/top-up` — Deposit funds
- `POST /api/v1/wallets/withdraw` — Withdraw funds
- `POST /api/v1/wallets/transfer` — Transfer to another user
- `GET /api/v1/wallets/transactions` — Paged transaction history (filterable, sortable)

**Entities**:
- `Wallet` — UserId (unique), Balance (decimal 18,2), Status (Active/Frozen/Closed)
- `Transaction` — WalletId (FK), Type, Amount, BalanceBefore, BalanceAfter, Reference, Description, CreatedAt

**Key features**:
- One wallet per user (unique index on UserId)
- Immutable transaction ledger (BalanceBefore/BalanceAfter recorded)
- Atomic transfers with Serializable isolation (prevents race conditions)
- Server-side pagination with search, type filter, date range, sorting (LINQ + optional stored procedure)
- Receiver validation via AuthService HTTP call

**Port**: `7261`

### API Gateway (Ocelot)

**Responsibility**: Single entry point, route multiplexing, Swagger aggregation.

**Features**:
- Routes `/api/v1/auth/*` → AuthService (7213)
- Routes `/api/v1/wallets/*` → WalletService (7261)
- Forwards Authorization header to downstream services
- Aggregates Swagger docs from both services
- Provides `/health` and root endpoints

**Port**: `7265`

**Note**: Token validation is performed by each downstream service, not at the gateway.

## Key Features

### Authentication Flow

1. **Registration**
   - `RegisterRequest` validated (email regex, phone, password length ≥ 8)
   - Email/phone uniqueness checked (database lookup)
   - Password hashed using ASP.NET Identity PasswordHasher
   - User created with default "User" role
   - Returns `UserResponse` (no password hash exposed)

2. **Login**
   - `LoginRequest` validated
   - User looked up by email
   - Password verified (PasswordHasher.VerifyPassword)
   - User must be active (IsActive == true)
   - JWT issued via `JwtTokenService.GenerateToken`
   - Token contains claims: NameIdentifier (user id), Name, Email, Role (optional)
   - Returns `LoginResponse` with token and expiry

3. **JWT Validation**
   - Configured via `AddJwtAuthentication` (JwtBearer scheme)
   - Key, Issuer, Audience from `appsettings.json` (JwtOptions)
   - Validates signature (symmetric key), expiry, issuer/audience
   - On success: HttpContext.User populated with ClaimsPrincipal
   - On failure: 401 Unauthorized (middleware short-circuits request)

### Wallet & Transfer Flow

1. **Create Wallet**
   - User (authenticated) can create one wallet per account
   - Wallet initialized with balance = 0, status = Active
   - Unique constraint on (UserId) enforced at DB level

2. **Top-up**
   - Amount validated (> 0, max 2 decimals)
   - Wallet must exist and be Active
   - DB transaction (Serializable) updates balance and writes Transaction record
   - Transaction captures Type=TopUp, BalanceBefore, BalanceAfter, Reference, Description

3. **Withdrawal**
   - Amount validated
   - Balance check ensures no overdraft (InsufficientBalanceException thrown)
   - DB transaction updates balance, writes Withdrawal transaction record

4. **Transfer (Atomic, Dual-sided)**
   - Sender id extracted from JWT claim (ClaimTypes.NameIdentifier)
   - TransferRequest validated (receiver id, amount, description)
   - Self-transfer prevented (sender == receiver check)
   - Receiver existence/active status validated via `AuthServiceClient.ValidateReceiverAsync` (HTTP GET to AuthService /api/v1/internal/users/{id})
   - Both wallets loaded and verified Active
   - Balance check on sender
   - DB transaction (IsolationLevel.Serializable) **atomically**:
	 - Updates sender balance (reduce by amount)
	 - Updates receiver balance (increase by amount)
	 - Writes paired Transaction entries:
	   - TransferDebit (sender wallet, Type=TransferDebit)
	   - TransferCredit (receiver wallet, Type=TransferCredit)
	 - Both entries share a unique Reference string (TXN-YYYYMMDD-{GUID})
   - Commit ensures both wallets and both transactions persist, or nothing is persisted
   - Any exception before Commit causes rollback

### Transaction History

**Query Parameters**:
- `pageNumber` (default 1) — 1-based page number
- `pageSize` (default 10, max 100) — items per page
- `search` (optional) — text search in Reference and Description (substring match)
- `type` (optional) — filter by TransactionType (TopUp, Withdrawal, TransferDebit, TransferCredit)
- `dateFrom`, `dateTo` (optional) — inclusive date range filter on CreatedAt
- `sortBy` (default "createdAt") — "createdAt" or "amount"
- `sortOrder` (default "desc") — "asc" or "desc"

**Retrieval Path**:
- Service calls `TransactionRepository.GetByWalletIdWithFiltersAsync` (LINQ path, live API)
  - Alternative: stored procedure `GetTransactionsForWallet` via `GetByWalletIdUsingStoredProcedureAsync` (exists, not default)
- Repository builds LINQ query with all filters, counts total, applies sort + paging
- EF Core translates to SQL with LIKE, WHERE, ORDER BY, OFFSET/FETCH
- Service maps Transaction entities to TransactionResponse DTOs and wraps in PagedResponse<T>

**Response**:
- PagedResponse<TransactionResponse> includes:
  - Data: list of transactions for requested page
  - PageNumber, PageSize, TotalRecords, TotalPages (computed)
  - Success, Message, Errors

## Database Design

### PaySphereAuthDb

| Table | Columns | Key Constraints |
|-------|---------|-----------------|
| **Roles** | Id (PK), Name (nvarchar 100), Description, CreatedAt, UpdatedAt | — |
| **Users** | Id (PK), FullName, Email (nvarchar 256, unique), PasswordHash (nvarchar 500), PhoneNumber, RoleId, IsActive, CreatedAt, UpdatedAt | FK RoleId → Roles.Id (Restrict) |

### PaySphereWalletDb

| Table | Columns | Key Constraints |
|-------|---------|-----------------|
| **Wallets** | Id (PK), UserId (int, unique), Balance (decimal 18,2), Status (int), CreatedAt, UpdatedAt | Unique(UserId) |
| **Transactions** | Id (PK), WalletId, Type (int), Amount (decimal 18,2), BalanceBefore (decimal 18,2), BalanceAfter (decimal 18,2), Reference (nvarchar 100, indexed), Description, CreatedAt, UpdatedAt | FK WalletId → Wallets.Id (Cascade) |

### Key Design Decisions

- **Separate databases**: AuthService and WalletService own independent databases (data ownership principle)
- **No cross-database FKs**: services use HTTP validation instead (loose coupling)
- **Code-First migrations**: fluent IEntityTypeConfiguration classes define schema; migrations applied at startup via `Database.MigrateAsync()`
- **Decimal(18,2)**: standard for currency (18 digits, 2 decimals = max ~9.2 trillion with precision to cent)
- **Ledger design**: Transaction records immutable (only insert, never update/delete) with BalanceBefore/BalanceAfter for audit trail

## API Endpoints

### AuthService

```
POST   /api/v1/auth/register                 — Register new user
POST   /api/v1/auth/login                    — Authenticate and issue JWT
GET    /api/v1/auth/profile                  — Get authenticated user profile [Authorize]
PUT    /api/v1/auth/change-password          — Update password [Authorize]
GET    /api/v1/internal/users/{id}           — Internal: validate receiver (used by WalletService)
GET    /health                               — Health check
```

### WalletService

```
POST   /api/v1/wallets                       — Create wallet [Authorize]
GET    /api/v1/wallets                       — Get wallet info [Authorize]
GET    /api/v1/wallets/balance               — Get current balance [Authorize]
POST   /api/v1/wallets/top-up                — Deposit [Authorize]
POST   /api/v1/wallets/withdraw              — Withdraw [Authorize]
POST   /api/v1/wallets/transfer              — Transfer to another user [Authorize]
GET    /api/v1/wallets/transactions          — Paged transaction history [Authorize]
GET    /health                               — Health check
```

### API Gateway

```
GET    /                                     — Gateway welcome message
GET    /health                               — Gateway health check
GET    /swagger/ui                           — Aggregated Swagger UI (auth + wallet)
```

All `/api/v1/*` routes are proxied to appropriate downstream service.

## Project Structure

```
PaySphere/
├── src/
│   ├── BuildingBlocks/
│   │   └── PaySphere.BuildingBlocks/
│   │       ├── Base/BaseEntity.cs                 — Id, CreatedAt, UpdatedAt
│   │       ├── Responses/ApiResponse.cs           — Unified response wrapper
│   │       ├── Responses/PagedResponse.cs         — Paged response with metadata
│   │       ├── Pagination/PaginationRequest.cs    — Page number/size with cap
│   │       ├── Exceptions/BaseException.cs        — Business error exception
│   │       ├── Enums/TransactionType.cs           — TopUp, Withdrawal, TransferDebit, TransferCredit
│   │       ├── Enums/WalletStatus.cs              — Active, Frozen, Closed
│   │       ├── Enums/RoleType.cs                  — Admin, User
│   │       └── Constants/ErrorMessages.cs         — Shared error strings
│   │
│   ├── Gateway/
│   │   └── PaySphere.ApiGateway/
│   │       ├── Program.cs                         — Setup Ocelot, Swagger aggregation
│   │       ├── ocelot.json                        — Route definitions
│   │       └── appsettings.json                   — Gateway config
│   │
│   ├── Services/
│   │   ├── PaySphere.AuthService/
│   │   │   ├── Program.cs                         — App startup
│   │   │   ├── Controllers/
│   │   │   │   ├── AuthController.cs              — Login, Register, Profile, ChangePassword
│   │   │   │   └── InternalUsersController.cs     — User validation for WalletService
│   │   │   ├── Services/
│   │   │   │   ├── AuthService.cs                 — Business logic
│   │   │   │   ├── JwtTokenService.cs             — Token generation
│   │   │   │   └── Interfaces/
│   │   │   ├── Repositories/
│   │   │   │   ├── GenericRepository.cs           — CRUD template
│   │   │   │   ├── UserRepository.cs              — User-specific queries
│   │   │   │   ├── RoleRepository.cs              — Role lookups
│   │   │   │   └── Interfaces/
│   │   │   ├── Data/
│   │   │   │   ├── PaySphereAuthDbContext.cs      — EF DbContext
│   │   │   │   ├── Configurations/
│   │   │   │   │   ├── UserConfiguration.cs       — Fluent API mapping
│   │   │   │   │   └── RoleConfiguration.cs
│   │   │   │   ├── Seed/DataSeeder.cs             — Initialize default roles/admin
│   │   │   │   └── Migrations/
│   │   │   ├── DTOs/
│   │   │   │   ├── Requests/
│   │   │   │   └── Responses/
│   │   │   ├── Entities/
│   │   │   │   ├── User.cs
│   │   │   │   └── Role.cs
│   │   │   ├── Validators/
│   │   │   │   ├── RegisterRequestValidator.cs    — Email regex, password length
│   │   │   │   ├── LoginRequestValidator.cs
│   │   │   │   └── ChangePasswordRequestValidator.cs
│   │   │   ├── Helpers/
│   │   │   │   └── PasswordHasher.cs              — Hashing/verification wrapper
│   │   │   ├── Middleware/
│   │   │   │   └── GlobalExceptionMiddleware.cs   — Exception → ApiResponse mapping
│   │   │   ├── Extensions/
│   │   │   │   ├── ServiceCollectionExtensions.cs — DI + Serilog + JWT setup
│   │   │   │   └── ApplicationBuilderExtensions.cs— Middleware registration
│   │   │   ├── Configurations/
│   │   │   │   └── JwtOptions.cs                  — Key, Issuer, Audience, ExpiryInMinutes
│   │   │   └── appsettings*.json
│   │   │
│   │   └── PaySphere.WalletService/
│   │       ├── Program.cs
│   │       ├── Controllers/WalletController.cs
│   │       ├── Services/
│   │       │   ├── WalletService.cs               — Transfer, TopUp, Withdraw, GetTransactions
│   │       │   ├── AuthServiceClient.cs           — HTTP call to AuthService
│   │       │   └── Interfaces/
│   │       ├── Repositories/
│   │       │   ├── WalletRepository.cs
│   │       │   ├── TransactionRepository.cs       — LINQ + stored proc paths
│   │       │   └── Interfaces/
│   │       ├── Data/
│   │       │   ├── WalletDbContext.cs
│   │       │   ├── Configurations/
│   │       │   │   ├── WalletConfiguration.cs
│   │       │   │   └── TransactionConfiguration.cs
│   │       │   └── Migrations/
│   │       ├── DTOs/
│   │       ├── Entities/
│   │       ├── Validators/
│   │       ├── Exceptions/
│   │       ├── Middleware/
│   │       ├── Extensions/
│   │       ├── Configurations/
│   │       └── appsettings*.json
│   │
│   └── BuildingBlocks/...        (as above)
│
└── tests/
	├── PaySphere.AuthService.Tests/
	│   └── AuthServiceTests.cs                    — NUnit + Moq
	│
	└── PaySphere.WalletService.Tests/
		├── WalletServiceTests.cs                  — Business logic (mocked repos)
		└── TransactionRepositoryTests.cs          — LINQ behavior (in-memory Sqlite)
```

## Testing

### AuthService Tests

**Framework**: NUnit + Moq

**Coverage**:
- Register: success, duplicate email/phone rejection, password hashing
- Login: valid credentials, token generation
- Profile: retrieval and authorization

**Approach**: All dependencies (IUserRepository, IRoleRepository, IJwtTokenService) are mocked; tests focus on service logic.

### WalletService Tests

**Framework**: NUnit + Moq + FluentAssertions + EF Core in-memory Sqlite

**Unit Tests** (WalletServiceTests.cs):
- Create wallet: success and duplicate rejection
- Get wallet/balance: success and WalletNotFound
- Top-up/Withdraw: balance updates, transaction generation, inactive wallet rejection, insufficient balance rejection, invalid amount rejection
- Transfer: paired debit/credit, same reference, self-transfer prevention, invalid receiver, insufficient balance, inactive wallet
- Transaction history: paged retrieval

**Approach**: Service under test with mocked repositories; provides real WalletDbContext (in-memory Sqlite) to support BeginTransactionAsync semantics.

**Repository Tests** (TransactionRepositoryTests.cs):
- Search (Reference/Description), type filter, date range, sorting (by CreatedAt or Amount), pagination (Skip/Take/CountAsync)
- Uses real EF Core/Sqlite to validate LINQ translation to SQL

**Test DB**: Microsoft.Data.Sqlite (in-memory connection)

## Running Locally

### Prerequisites

- .NET 8 SDK
- SQL Server (LocalDB or Express)
- Visual Studio 2022 or VS Code

### Setup

1. **Clone repository**
```
git clone https://github.com/asadali2004/PaySphere.git
cd PaySphere
```

2. **Configure connection strings**
Update `appsettings.json` in each service:
```json
{
  "ConnectionStrings": {
	"DefaultConnection": "Data Source=localhost\\SQLEXPRESS;Initial Catalog=PaySphereAuthDb;Integrated Security=True;Encrypt=True;TrustServerCertificate=True;"
  }
}
```

3. **Restore packages & build**
```bash
dotnet restore
dotnet build
```

4. **Run services** (each in separate terminal)
```bash
# AuthService (port 7213)
cd src/Services/PaySphere.AuthService
dotnet run

# WalletService (port 7261)
cd src/Services/PaySphere.WalletService
dotnet run

# API Gateway (port 7265)
cd src/Gateway/PaySphere.ApiGateway
dotnet run
```

5. **Run tests**
```bash
dotnet test
```

### Expected Output

- AuthService starts: "Listening on https://localhost:7213"
- WalletService starts: "Listening on https://localhost:7261"
- Gateway starts: "Listening on https://localhost:7265"

At startup:
- AuthService applies migrations and seeds default roles + admin user
- Serilog writes logs to console and `logs/auth-service-*.txt`, `logs/wallet-service-*.txt`

## Health Checks

### Gateway
```
GET https://localhost:7265/health
→ { "status": "Healthy" }
```

### AuthService
```
GET https://localhost:7213/health
→ { "status": "Healthy" }
```

### WalletService
```
GET https://localhost:7261/health
→ { "status": "Healthy" }
```

## Swagger

**Aggregated Swagger UI** (via gateway):
```
https://localhost:7265/swagger/ui
```
Displays combined documentation for AuthService and WalletService.

**Individual service Swagger**:
- AuthService: https://localhost:7213/swagger/ui
- WalletService: https://localhost:7261/swagger/ui

## Security Considerations

### Current Implementation

- **Password hashing**: ASP.NET Identity PasswordHasher (salted, PBKDF2)
- **JWT**: Symmetric key (HS256) in appsettings (dev key shown)
- **Database access**: EF Core parameterized queries (SQL injection prevention)
- **Authorization**: [Authorize] attribute on protected endpoints
- **HTTPS**: Enforced via `UseHttpsRedirection()`
- **CORS**: NOT configured (same-origin or add as needed)

### Production Hardening Needed

1. **JWT Key Management**
   - Move key to Azure Key Vault or secrets manager
   - Use asymmetric key (RS256) with public/private key pair
   - Rotate keys periodically

2. **API Security**
   - Add rate limiting (e.g., AspNetCoreRateLimit)
   - Add CORS with specific origin whitelist
   - Add request validation + sanitization
   - Consider API key authentication for internal endpoints (InternalUsersController)

3. **Database**
   - Use SQL Server Always Encrypted for PasswordHash column
   - Implement row-level security (RLS) per tenant
   - Enable database audit logging

4. **Audit & Monitoring**
   - Enhance logging to capture who accessed what (user id, timestamp, action)
   - Set up alerts for suspicious patterns (multiple failed logins, large transfers)
   - Archive audit logs to external storage

5. **Compliance**
   - PCI DSS for payment data (if storing card info)
   - GDPR for user data (right to be forgotten, data export)

## Future Improvements

- [ ] Refresh token support (JWT expiry rotation)
- [ ] Multi-factor authentication (MFA)
- [ ] Role-based access control (RBAC) enforcement (Role claim in policies)
- [ ] Wallet freeze/close operations
- [ ] Transaction reversal/refund workflow
- [ ] Notification service (email/SMS on transfers)
- [ ] Admin dashboard (user management, transaction monitoring)
- [ ] Rate limiting and DDoS protection
- [ ] Distributed tracing (OpenTelemetry)
- [ ] GraphQL API alongside REST
- [ ] Kubernetes deployment (Helm charts)
- [ ] Load balancing and auto-scaling
- [ ] Event sourcing for audit trail
- [ ] Saga pattern for distributed transactions

## Contributing

Contributions welcome! Please:
1. Fork the repository
2. Create a feature branch
3. Commit with clear messages
4. Submit a pull request with description

## License

MIT License — see LICENSE file

## Author

**Asad Ali**  
[GitHub](https://github.com/asadali2004)  
[Email](mailto:your-email@example.com)

---

## Quick Reference

| Concept | File / Class |
|---------|-------------|
| JWT Issuance | `JwtTokenService.GenerateToken` |
| JWT Validation | `AddJwtAuthentication` (JwtBearer middleware) |
| Transfer Atomicity | `WalletService.TransferAsync` (BeginTransactionAsync) |
| Receiver Validation | `AuthServiceClient.ValidateReceiverAsync` |
| Password Hashing | `PasswordHasher.HashPassword / VerifyPassword` |
| Error Handling | `GlobalExceptionMiddleware` |
| API Response Wrapper | `ApiResponse<T>` |
| Paged Results | `PagedResponse<T>` |
| Transactions Ledger | `Transaction` entity (BalanceBefore/BalanceAfter) |
| Logging | Serilog (console + rolling files) |
| Routing | `ocelot.json` |

---

**Last updated**: 2026-08-13  
**Status**: Production-ready
