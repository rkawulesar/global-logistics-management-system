# Global Logistics Management System

A full-stack .NET application for managing logistics clients, contracts, and service requests. Built as an academic project and adapted for this portfolio, it demonstrates a separate MVC frontend and REST API, database persistence, design patterns, and automated testing.

## Features

- **Client management** : Create, view, update, and delete clients, with safeguards for linked contracts.
- **Contract management** : Track contract statuses, filter by service level and dates, and upload PDF agreements up to 10 MB.
- **Service requests** : Manage requests linked to contracts, prevent duplicates, and track progress.
- **Currency conversion** : Calculate ZAR costs from USD using ExchangeRate-API.
- **Notifications** : Generate notifications when service request statuses change.
- **Role-based access** : Separate Admin and Client access to application records using JWT authentication.

## Technology Stack

| Area | Technologies |
| --- | --- |
| Frontend | ASP.NET Core MVC, Razor, HTML, CSS, JavaScript |
| Backend | C#, ASP.NET Core Web API, .NET 9 |
| Database | SQL Server, Entity Framework Core |
| Authentication | JWT, MVC sessions |
| Testing | NUnit, Moq, HTTP integration tests |
| Tooling | Docker Compose, GitHub Actions, Swagger / OpenAPI |

## Architecture

```text
MVC Frontend → API Services → Web API → Business Services → Repositories → SQL Server
```

The frontend communicates with the API through typed `HttpClient` services. A custom `JwtTokenHandler` attaches the session-held token to API requests.

| Project | Responsibility |
| --- | --- |
| `GLMS/GLMS_Web` | MVC frontend and API client services |
| `GLMS/GLMS_API` | Authentication, business logic, document handling, and database access |
| `GLMS/GLMS_Tests` | Unit and API integration tests |

The solution uses four design patterns:

- **Repository:** Separates persistence from business logic.
- **Factory:** Creates contracts in different workflow states.
- **Strategy:** Separates currency calculations from their calling code.
- **Observer:** Handles service request status notifications.

## Running Locally

**Requirements:** .NET 9 SDK, SQL Server, and an ExchangeRate-API key for live currency conversion.

1. Create a development database and run [`PROG7311 SCRIPT DB.sql`](PROG7311%20SCRIPT%20DB.sql). The script includes fictional sample clients. It does not provision the `Users` table or an administrator; create these to match the API's `User` model and `GlmsContext`, using an ASP.NET Core Identity password hash.
2. Configure the API through environment variables or .NET user secrets:

   | Setting | Value |
   | --- | --- |
   | `ConnectionStrings__DefaultConnection` | Your SQL Server connection string |
   | `Jwt__Key` | A signing secret generated from at least 32 random bytes |
   | `ExchangeRateApi__ApiKey` | Your exchange-rate provider key |

3. Restore and build from the repository root:

   ```powershell
   dotnet restore GLMS/GLMS.slnx
   dotnet build GLMS/GLMS.slnx --configuration Release
   ```

4. Run the API and frontend in separate terminals:

   ```powershell
   dotnet run --project GLMS/GLMS_API --launch-profile https
   ```

   ```powershell
   dotnet run --project GLMS/GLMS_Web --launch-profile https
   ```

Open the frontend at `https://localhost:7206` or Swagger at `https://localhost:7134/swagger`. Local HTTPS requires a trusted .NET development certificate.

Docker configuration is included in [`GLMS/docker-compose.yml`](GLMS/docker-compose.yml), with environment placeholders in [`GLMS/.env.example`](GLMS/.env.example). Container setup requires database provisioning and an API health-check dependency adjustment: the configured probe uses `curl`, which is absent from the runtime image. A Docker `.env` file is not loaded by `dotnet run`.

Keep credentials and populated `.env` files out of source control. This project is intended as a local portfolio demonstration.

## Testing

The test project contains **19 unit tests** covering currency calculations, file validation, and service request workflows, plus **7 API integration tests**.

```powershell
dotnet test GLMS/GLMS_Tests/GLMS_Tests.csproj --configuration Release --filter "FullyQualifiedName!~IntegrationTests"
```

Integration tests require a running API at `https://localhost:7134`, a disposable database with client ID `1`, and an administrator account. Set `GLMS_TEST_ADMIN_USERNAME` and `GLMS_TEST_ADMIN_PASSWORD`, then run:

```powershell
dotnet test GLMS/GLMS_Tests/GLMS_Tests.csproj --configuration Release --filter "FullyQualifiedName~IntegrationTests"
```

