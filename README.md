# TaskManager.Api

An educational ASP.NET Core Web API for publishing tasks and managing participation. Employers create tasks, users join them directly or submit requests, and administrators review applications for the employer role.

This is a backend-only portfolio project focused on authentication, role-based access, business rules and relational data modeling.

## Features

- Registration and login with JWT authentication.
- Public task browsing, including descriptions, checklists and participants.
- Employer-owned tasks with open participation or owner-approved join requests.
- Employer role applications reviewed by an administrator.
- Personal profiles and lists of created or joined tasks.
- Administrative user lookup, updates and deletion.
- Console and daily file logging through Serilog.

## Technology Stack

C# / .NET 8, ASP.NET Core Web API, ASP.NET Core Identity, JWT Bearer authentication, Entity Framework Core 8, SQLite, Swagger/OpenAPI and Serilog.

## Roles and Workflow

| Role | Main responsibilities |
| --- | --- |
| User | Manage a personal profile, participate in tasks and apply for the employer role. |
| Employer | Create and delete owned tasks and review requests to join them. |
| Admin | Review employer role applications and manage user accounts. |

A typical scenario:

1. Register an account and submit an employer role application.
2. Sign in as the seeded administrator and approve the application.
3. Sign in again as the approved user to obtain a JWT containing the Employer role.
4. Create a task with direct joining enabled or with approval required.
5. Register a separate participant account and join the task or submit a join request.
6. For tasks requiring approval, sign in as the owner and approve the request.

Current business rules:

- Employers cannot participate in tasks as performers.
- An employer application is rejected if the applicant is already participating in tasks.
- Each user can submit only one employer application, including after rejection.
- Each user can submit only one join request per task, including after rejection.
- Only pending requests can be approved or rejected; task join requests are reviewed by the task owner.

## Run Locally

### Prerequisites

- .NET 8 SDK and Git.
- The EF Core CLI tool (`dotnet-ef`), version 8.x.
- PowerShell for the commands below.

SQLite runs locally; no separate database server is required.

### 1. Clone and restore

```powershell
git clone https://github.com/viktorcit/TaskManager.Api.git
cd TaskManager.Api/TaskManager.Api
dotnet restore
```

Run the remaining commands from this directory, which contains `TaskManager.Api.csproj`.

If the EF Core CLI is not installed:

```powershell
dotnet tool install --global dotnet-ef --version 8.0.0
```

### 2. Configure local settings

The project already has a User Secrets ID. Replace the placeholders below before running the commands. Use a randomly generated JWT signing secret of at least 32 bytes and your own administrator password.

```powershell
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet user-secrets set "JwtSettings:SecretKey" "<your-random-signing-secret>"
dotnet user-secrets set "AdminUsername" "LocalAdmin"
dotnet user-secrets set "AdminPassword" "<your-admin-password>"
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Data Source=taskmanager.local.db"
```

The administrator password must satisfy the default Identity policy: at least six characters, with uppercase, lowercase, a digit and a non-alphanumeric character.

These settings are stored outside the repository. User Secrets are for local development, not a production secret store. Do not commit credentials or your local database.

Issuer and audience are configured in `appsettings.json` under `Jwt:Issuer` and `Jwt:Audience`. The signing secret uses the separate key `JwtSettings:SecretKey`.

The connection string above creates a separate local database instead of using the tracked `app.db`. Choose another unused filename if you already have a database with that name.

### 3. Create the database and start the API

```powershell
dotnet ef database update
dotnet dev-certs https --trust
dotnet run --launch-profile https
```

Apply migrations before starting the application. At startup, the application creates missing roles and the configured administrator account if that username does not already exist. Changing `AdminPassword` does not reset an existing account's password.

Open [Swagger UI](https://localhost:7016/swagger). The HTTPS launch profile uses `https://localhost:7016` and enables the Development environment.

> The refactor replaced the previous migration history. These instructions target a fresh database. An older database requires a separate data migration or recreation; do not apply the new initial migration to it directly.

## Using the API

Swagger UI exposes the available operations and DTO schemas. The generated OpenAPI document is available at [swagger/v1/swagger.json](https://localhost:7016/swagger/v1/swagger.json). Both are enabled only in Development.

### Public endpoints

| Method | Route | Purpose |
| --- | --- | --- |
| POST | `/auth/register` | Register a user and receive a JWT. |
| POST | `/auth/login` | Sign in and receive a JWT. |
| GET | `/tasks` | List tasks with the InProgress status. |
| GET | `/tasks/{taskId}` | Retrieve task details. |

Other operations are grouped under `/profile`, `/users`, `/employer-requests`, `/tasks` and `/request-to-task`.

### Authenticated requests

Registration and login return the token itself, not an object with a `token` property. Send its value without surrounding quotes in the header:

```http
Authorization: Bearer <token>
```

Tokens expire after one hour. Sign in again after a role change to obtain a token with the updated roles.

Swagger currently has no Bearer authorization configuration. Use an HTTP client such as Postman, curl or your IDE's HTTP client to send authenticated requests.

## Project Structure

```text
TaskManager.Api/
|-- Controllers/       HTTP endpoints grouped by functional area
|-- Services/          Business logic and database operations
|-- Interfaces/        Service contracts
|-- Entity/            Persistence entities, including TaskPerformer
|-- Data/
|   |-- Contracts/     Shared data shapes
|   |-- DTO/           Request, response and service result DTOs
|   |-- AppDbContext.cs
|   `-- Seed.cs        Role and administrator initialization
|-- Enums/             Task, request and response classifications
|-- Extensions/        Claims principal helpers
|-- Helpers/           Response factory, role names and error messages
|-- Migrations/        EF Core migrations
`-- Program.cs         Dependency injection and HTTP pipeline
```

Controllers delegate application operations to services. Services use EF Core and ASP.NET Core Identity directly; the application is organized within a single web project.

## Scope

This is a learning project, not a production-ready service. It has no frontend or automated test suite. Task completion/cancellation and token refresh are not exposed as API operations.

## License

[MIT](LICENSE.txt).
