# TaskManager.Api

TaskManager API is a REST API for managing user tasks, users, roles, and the assignment of each role and its tasks.

## Tech stacks:

- ASP.NET
- ASP.NET Identity
- EF Core
- SQLite
- JWT Token
- C#
- SOLID
- Swagger

## How to run

### Requirements

- .NET SDK 8.0+
- Git

### Setup

1. Clone the repository:

```bash
git clone <repository-url>
cd <repository-folder>
```

2. Restore dependencies:

```bash
dotnet restore
```

3. Apply migrations:

```bash
dotnet ef database update
```

### Start the application

```bash
dotnet run --project TaskManager.Api
```

can also starting via Visual studio.

The project uses SQLite, so no external database server is required.

## Project structure

- Controllers: API controllers responsible for processing HTTP requests.
- Data: database context and Seed.
- DTOs: DTOs of the project responsible for accepting or returning data.
- Migrations: EF core migrations for database management.
- Model: project entityes classes.
- Services: business logic and services used by controllers.

## Authentication and Roles

### Get JWT Token

Obtain a JWT token via:

- `POST /account/register`
- `POST /account/login`

### Roles

- User
- Admin
- Employer

### Endpoints for admins and employers only

#### Admin:

- `GET /admin/employer-requests`
- `GET /admin/employer-requests/{id}`
- `GET /users`
- `GET /users/{id}`
- `POST /admin/employer-requests/{id}/approve`
- `POST /admin/employer-requests/{id}/reject`
- `PATCH /users/{id}`
- `DELETE /users/{id}`

#### Employers:

- `GET /request-to-join/requests-join-to-task`
- `GET /request-to-join/requests-join-to-task/{id}`
- `GET /tasks/my-created`
- `POST /request-to-join/requests-join-to-task/{id}/approve`
- `POST /request-to-join/requests-join-to-task/{id}/reject`
- `POST /tasks/create`
- `DELETE /tasks/delete/{id}`

### Available for all authenticated users (needs Authorization header: Bearer <token>):

- `GET /profile`
- `GET /profile/{nickname}`
- `GET /profile/my`
- `GET /tasks/my-performing`
- `GET /tasks/my-performing/{id}`
- `POST /profile/my`
- `POST /account/request-employer`
- `POST /tasks/join/{id}`
- `POST /tasks/join-request/{id}`
- `PATCH /profile`
- `DELETE /profile`

### Public endpoints (These endpoints can be accessed without authentication.)

- `GET /tasks`
- `GET /tasks/all`
- `GET /tasks/{id}`
- `POST /account/register`
- `POST /account/login`

## List of endpoints

### Admin

- `GET /admin/employer-requests`
  **Description:** Get a list of requests for the role of employer

  **Roles:**
  - Admin

  **Response codes:**
  - 200 OK
  - 401 Unauthorized
  - 403 Forbidden

  **Responses:**

  ```json
  [
    {
      "id": 0,
      "userId": "string",
      "companyName": "string",
      "createdAt": "2026-03-28T18:54:41.795Z",
      "updatedAt": "2026-03-28T18:54:41.795Z",
      "status": "string"
    }
  ]
  ```

- `GET /admin/employer-requests/{id}`
  **Description:** Get a specific request for an employer role by ID

  **Roles:**
  - Admin

  **Response codes:**
  - 200 OK
  - 401 Unauthorized
  - 403 Forbidden
  - 404 Not Found

  **Responses:**

  ```json
  {
    "id": 0,
    "userId": "string",
    "companyName": "string",
    "createdAt": "2026-03-28T18:57:36.249Z",
    "updatedAt": "2026-03-28T18:57:36.249Z",
    "status": "string",
    "description": "string"
  }
  ```

- `POST /admin/employer-requests/{id}/approve`
  **Description:** Accept the request for employer role

  **Roles:**
  - Admin

  **Response codes:**
  - 200 OK
  - 400 Bad Request
  - 401 Unauthorized
  - 403 Forbidden
  - 404 Not Found
  - 500 Internal Server Error

  **Request body:**

  ```json
  {
    "adminComment": "stringstringstringstringstringstringstringstringst"
  }
  ```

  **Responses:**

  ```json
  {
    "status": "string",
    "id": 0,
    "userId": "string"
  }
  ```

- `POST /admin/employer-requests/{id}/reject`
  **Description:** Rejection of a request for an employer role

  **Roles:**
  - Admin

  **Response codes:**
  - 200 OK
  - 401 Unauthorized
  - 403 Forbidden
  - 404 Not Found

  **Request body:**

  ```json
  {
    "reason": "stringstringstringstringstringstringstringstringst"
  }
  ```

  **Responses:**
  - 200 OK - empty response body

### Auth

- `POST /account/register`
  **Description:** Register an account and receive a token

  **Roles:** For unregistered users

  **Response codes:**
  - 200 OK
  - 400 Bad Request
  - 409 Conflict
  - 500 Internal Server Error

  **Request body:**

  ```json
  {
    "name": "string",
    "nickname": "string",
    "age": 100,
    "password": "string"
  }
  ```

  **Responses:**

  ```json
  {
    "token": "string"
  }
  ```

- `POST /account/login`
  **Description:** Login to your account and receive a token

  **Roles:** For unregistered users

  **Response codes:**
  - 200 OK
  - 400 Bad Request
  - 401 Unauthorized
  - 403 Forbidden

  **Request body:**

  ```json
  {
    "nickname": "string",
    "password": "string"
  }
  ```

  **Responses:**

  ```json
  {
    "token": "string"
  }
  ```

- `POST /account/request-employer`
  **Description:** Request for an employer role

  **Roles:**
  - User

  **Response codes:**
  - 200 OK
  - 400 Bad Request
  - 401 Unauthorized

  **Request body:**

  ```json
  {
    "companyName": "string",
    "website": "string",
    "description": "stringstringstringstringstringstringstringstringst"
  }
  ```

  **Responses:**

  ```json
  {
    "companyName": "string",
    "website": "string",
    "description": "stringstringstringstringstringstringstringstringst"
  }
  ```

  ### Profile

- `GET /profile`
  **Description:** get all profiles

  **Roles:**
  - User
  - Employer
  - Admin

  **Response codes:**
  - 200 OK
  - 401 Unauthorized

  **Response:**

  ```json
  [
    {
      "id": "string",
      "userName": "string",
      "normalizedUserName": "string",
      "email": "string",
      "normalizedEmail": "string",
      "emailConfirmed": true,
      "passwordHash": "string",
      "securityStamp": "string",
      "concurrencyStamp": "string",
      "phoneNumber": "string",
      "phoneNumberConfirmed": true,
      "twoFactorEnabled": true,
      "lockoutEnd": "2026-03-29T13:04:53.948Z",
      "lockoutEnabled": true,
      "accessFailedCount": 0,
      "name": "string",
      "nickname": "string"
    }
  ]
  ```

- `GET /profile/{nickname}`
  **Description:** Get a specific profile by nickname

  **Roles:**
  - User
  - Admin
  - Employer

  **Response codes:**
  - 200 OK
  - 401 Unauthorized
  - 404 Not Found

  **Responses:**

  ```json
  {
    "id": "string",
    "userName": "string",
    "normalizedUserName": "string",
    "email": "string",
    "normalizedEmail": "string",
    "emailConfirmed": true,
    "passwordHash": "string",
    "securityStamp": "string",
    "concurrencyStamp": "string",
    "phoneNumber": "string",
    "phoneNumberConfirmed": true,
    "twoFactorEnabled": true,
    "lockoutEnd": "2026-03-29T13:34:22.360Z",
    "lockoutEnabled": true,
    "accessFailedCount": 0,
    "name": "string",
    "nickname": "string"
  }
  ```

- `GET /profile/my`
  **Description:** Get your profile

  **Roles:**
  - Admin
  - User
  - Employer

  **Response codes:**
  - 200 OK
  - 401 Unauthorized

  **Responses:**

  ```json

  ```

- `PATCH /profile`
  **Description:**

  **Roles:**

  **Response codes:**

  **Request body:**

  ```json

  ```

  **Responses:**

- `DELETE /profile`
  **Description:**

  **Roles:**

  **Response codes:**

  **Request body:**

  ```json

  ```

  **Responses:**
