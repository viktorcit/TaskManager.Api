# TaskManager.Api

TaskManager API is a RESTful backend application that allows users to create, manage, and participate in tasks with role-based access control (User, Employer, Admin).

## Features include:

- Task creation and assignment
- Role-based permissions
- Request system for joining tasks
- Employer approval workflow
- Moderation by admin
- Request system for role employer

## Tech stacks:

- ASP.NET Core Web API
- Entity Framework Core
- ASP.NET Identity (Authentication & Authorization)
- JWT Bearer Authentication
- SQLite (Database)
- Swagger (OpenAPI)
- C#
- SOLID Principles

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

The application can also be started via Visual Studio.

The project uses SQLite, so no external database server is required.

## API Documentation

Swagger UI (available after running the project):

- Swagger UI: https://localhost:<port>/swagger
- Full API description: see sections below

## Project structure

- Controllers: API controllers responsible for processing HTTP requests.
- Data: database context and Seed.
- DTOs: Data transfer objects used for requests and responses.
- Migrations: EF core migrations for database management.
- Models: Entity classes of the project.
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

- `GET /admin/employer-requests/{id}`

  **Description:** Get a specific request for an employer role by ID

  **Roles:**
  - Admin

- `POST /admin/employer-requests/{id}/approve`

  **Description:** Accept the request for employer role

  **Roles:**
  - Admin

- `POST /admin/employer-requests/{id}/reject`

  **Description:** Rejection of a request for an employer role

  **Roles:**
  - Admin

### Auth

- `POST /account/register`

  **Description:** Register an account and receive a token

  **Roles:** For unregistered users

- `POST /account/login`

  **Description:** Login to your account and receive a token

  **Roles:** For unregistered users

- `POST /account/request-employer`

  **Description:** Request for an employer role

  **Roles:**
  - User

### Profile

- `GET /profile`

  **Description:** Get all profiles

  **Roles:**
  - User
  - Employer
  - Admin

- `GET /profile/{nickname}`

  **Description:** Get a specific profile by nickname

  **Roles:**
  - User
  - Admin
  - Employer

- `GET /profile/my`

  **Description:** Get your profile

  **Roles:**
  - Admin
  - User
  - Employer

- `PATCH /profile`

  **Description:** Update your profile

  **Roles:**
  - User
  - Employer

- `DELETE /profile`

  **Description:** Delete your profile

  **Roles:**
  - User
  - Employer

### RequestToJoin

- `GET /request-to-join/requests-join-to-task`

  **Description:** Get a list of requests to join the task you created.

  **Roles:**
  - Employer

- `GET /request-to-join/requests-join-to-task/{id}`

  **Description:** Get a specific request to join the task you created

  **Roles:**
  - Employer

- `POST /request-to-join/requests-join-to-task/{id}/approve`

  **Description:** Approve a request to join your task

  **Roles:**
  - Employer

- `POST /request-to-join/requests-join-to-task/{id}/reject`

  **Description:** Reject a request to join your task

  **Roles:**
  - Employer

### Task

- `GET /tasks`

  **Description:** Get all tasks with the "in progress" status

  **Roles:**
  - All users(including unauthenticated)

- `GET /tasks/all`

  **Description:** Get all tasks with any status

  **Roles:**
  - All users(including unauthenticated)

- `GET /tasks/{id}`

  **Description:** Get a task by ID

  **Roles:**
  - All users(including unauthenticated)

- `GET /tasks/my-created`

  **Description:** Get user-created tasks

  **Roles:**
  - Employer

- `GET /tasks/my-performing`

  **Description:** Get the tasks you are performing

  **Roles:**
  - User

- `GET /tasks/my-performing/{id}`

  **Description:** Get the task you are performing by ID

  **Roles:**
  - User

- `POST /tasks/create`

  **Description:** Create a task

  **Roles:**
  - Employer

- `POST /tasks/join/{id}`

  **Description:** Join a task if it is open

  **Roles:**
  - User

- `POST /tasks/join-request/{id}`

  **Description:** Create a request to join a task if it is closed

  **Roles:**
  - User

- `DELETE /tasks/delete/{id}`

  **Description:** Delete your task

  **Roles:**
  - Employer

### User

- `GET /users`

  **Description:** Get list of users

  **Roles:**
  - Admin

- `GET /users/{id}`

  **Description:** Get a user by ID

  **Roles:**
  - Admin

- `PATCH /users/{id}`

  **Description:** Change user by ID

  **Roles:**
  - Admin

- `DELETE /users/{id}`

  **Description:** Delete user by ID

  **Roles:**
  - Admin

## Request bodies

### `POST /account/register`

```json
{
  "name": "string",
  "nickname": "string",
  "age": 100,
  "password": "string"
}
```

### `POST /account/login`

```json
{
  "nickname": "string",
  "password": "string"
}
```

### `POST /account/request-employer`

```json
{
  "companyName": "string",
  "website": "string",
  "description": "string"
}
```

### `POST /admin/employer-requests/{id}/approve`

```json
{
  "adminComment": "string"
}
```

### `POST /admin/employer-requests/{id}/reject`

```json
{
  "reason": "string"
}
```

### `PATCH /profile`

```json
{
  "name": "string",
  "age": 100
}
```

### `POST /request-to-join/requests-join-to-task/{id}/approve`

```json
{
  // no request body
}
```

### `POST /request-to-join/requests-join-to-task/{id}/reject`

```json
{
  // no request body
}
```

### `POST /tasks/create`

```json
{
  "title": "string",
  "description": "string",
  "dueDate": "2026-04-02T19:08:10.129Z",
  "canAnyoneJoin": true,
  "performersId": ["string"]
}
```

### `POST /tasks/join-request/{id}`

```json
{
  "description": "string"
}
```

### `PATCH /users/{id}`

```json
{
  "name": "string",
  "nickname": "string",
  "age": 0
}
```

## Full API Documentation

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
  - 403 Forbidden

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
  {
    "id": "string",
    "name": "string",
    "nickname": "string",
    "age": 0,
    "createdAt": "2026-03-29T22:45:20.210Z",
    "ownerTasks": [
      {
        "id": 0,
        "title": "string",
        "status": 0
      }
    ],
    "performerTasks": [
      {
        "id": 0,
        "title": "string",
        "status": 0
      }
    ]
  }
  ```

- `PATCH /profile`

  **Description:** Update your profile

  **Roles:**
  - User
  - Employer

  **Response codes:**
  - 200 OK
  - 400 Bad Request
  - 401 Unauthorized
  - 403 Forbidden

  **Request body:**

  ```json
  {
    "name": "string",
    "age": 100
  }
  ```

  **Responses:**

  ```json
  {
    "name": "string",
    "age": 100
  }
  ```

- `DELETE /profile`

  **Description:** Delete your profile

  **Roles:**
  - User
  - Employer

  **Response codes:**
  - 204 No Content
  - 403 Forbidden
  - 401 Unauthorized

  **Responses:**
  - 204 No Content - empty response body

  ### RequestToJoin

- `GET /request-to-join/requests-join-to-task`

  **Description:** Get a list of requests to join the task you created.

  **Roles:**
  - Employer

  **Response codes:**
  - 200 OK
  - 401 Unauthorized
  - 403 Forbidden

  **Responses:**

  ```json
  [
    {
      "id": 0,
      "taskId": 0,
      "userId": "string",
      "userName": "string",
      "description": "string",
      "status": 0,
      "createdAt": "2026-03-29T23:05:38.227Z"
    }
  ]
  ```

- `GET /request-to-join/requests-join-to-task/{id}`

  **Description:** Get a specific request to join the task you created

  **Roles:**
  - Employer

  **Response codes:**
  - 200 OK
  - 401 Unauthorized
  - 403 Forbidden
  - 404 Not Found

  **Responses:**

  ```json
  {
    "id": 0,
    "taskId": 0,
    "userId": "string",
    "userName": "string",
    "description": "string",
    "status": 0,
    "createdAt": "2026-03-29T23:10:25.344Z"
  }
  ```

- `POST /request-to-join/requests-join-to-task/{id}/approve`

  **Description:** approve a request to join your task

  **Roles:**
  - Employer

  **Response codes:**
  - 200 OK
  - 400 Bad Request
  - 401 Unauthorized
  - 403 Forbidden
  - 404 Not Found

  **Responses:**

  ```json
  "message": "Request has been approved."
  ```

- `POST /request-to-join/requests-join-to-task/{id}/reject`

  **Description:** reject a request to join your task

  **Roles:**
  - Employer

  **Response codes:**
  - 200 OK
  - 401 Unauthorized
  - 404 Not Found

  **Responses:**

  ```json
  "message": "Request has been rejected."
  ```

  ### Task

- `GET /tasks`

  **Description:** Get all tasks with the "in progress" status

  **Roles:**
  - All users(unregistered too)

  **Response codes:**
  - 200 OK

  **Responses:**

  ```json
  [
    {
      "id": 0,
      "title": "string",
      "ownerId": "string",
      "ownerUsername": "string",
      "createdAt": "2026-04-01T20:25:07.092Z",
      "description": "string",
      "status": 0
    }
  ]
  ```

- `GET /tasks/all`

  **Description:** Get all tasks with any status

  **Roles:**
  - All users(unregistered too)

  **Response codes:**
  - 200 OK

  **Responses:**

  ```json
  [
    {
      "id": 0,
      "title": "string",
      "ownerId": "string",
      "ownerUsername": "string",
      "createdAt": "2026-04-01T20:27:02.299Z",
      "description": "string",
      "status": 0
    }
  ]
  ```

- `GET /tasks/{id}`

  **Description:** Get a task by ID

  **Roles:**
  - All users(unregistered too)

  **Response codes:**
  - 200 OK
  - 404 Not Found

  **Responses:**

  ```json
  {
    "id": 0,
    "title": "string",
    "description": "string",
    "dueDate": "2026-04-02T18:32:46.930Z",
    "createdAt": "2026-04-02T18:32:46.930Z",
    "updatedAt": "2026-04-02T18:32:46.930Z",
    "status": 0,
    "completedAt": "2026-04-02T18:32:46.930Z",
    "canAnyoneJoin": true,
    "ownerId": "string",
    "owner": {
      "id": "string",
      "name": "string",
      "username": "string"
    },
    "ownerUsername": "string",
    "performers": [
      {
        "id": "string",
        "name": "string",
        "username": "string"
      }
    ],
    "": ["string"]
  }
  ```

- `GET /tasks/my-created`

  **Description:** get user-created tasks

  **Roles:**
  - Employer

  **Response codes:**
  - 200 OK
  - 404 Not Found
  - 401 Unauthorized
  - 403 Forbidden

  **Responses:**

  ```json
  [
    {
      "id": 0,
      "title": "string",
      "ownerId": "string",
      "ownerUsername": "string",
      "createdAt": "2026-04-02T18:45:58.537Z",
      "description": "string",
      "status": 0
    }
  ]
  ```

- `GET /tasks/my-performing`

  **Description:** get the tasks you are performing

  **Roles:**
  - User

  **Response codes:**
  - 200 OK
  - 401 Unauthorized
  - 403 Forbidden
  - 404 Not Found

  **Responses:**

  ```json
  [
    {
      "id": 0,
      "title": "string",
      "ownerId": "string",
      "ownerUsername": "string",
      "createdAt": "2026-04-02T18:48:11.066Z",
      "description": "string",
      "status": 0
    }
  ]
  ```

- `GET /tasks/my-performing/{id}`

  **Description:** Get the task you are performing by ID

  **Roles:**
  - User

  **Response codes:**
  - 200 OK
  - 401 Unauthorized
  - 403 Forbidden
  - 404 Not Found

  **Responses:**

  ```json
  {
    "id": 0,
    "title": "string",
    "description": "string",
    "dueDate": "2026-04-02T19:05:23.961Z",
    "createdAt": "2026-04-02T19:05:23.961Z",
    "updatedAt": "2026-04-02T19:05:23.961Z",
    "status": 0,
    "completedAt": "2026-04-02T19:05:23.961Z",
    "canAnyoneJoin": true,
    "ownerId": "string",
    "owner": {
      "id": "string",
      "name": "string",
      "username": "string"
    },
    "ownerUsername": "string",
    "performers": [
      {
        "id": "string",
        "name": "string",
        "username": "string"
      }
    ],
    "performersId": ["string"]
  }
  ```

- `POST /tasks/create`

  **Description:** create a task

  **Roles:**
  - Employer

  **Response codes:**
  - 200 OK
  - 400 Bad Request
  - 401 Unauthorized
  - 403 Forbidden

  **Request body:**

  ```json
  {
    "title": "string",
    "description": "string",
    "dueDate": "2026-04-02T19:08:10.129Z",
    "canAnyoneJoin": true,
    "performersId": ["string"]
  }
  ```

  **Responses:**

  ```json
  {
    "id": 0,
    "title": "string",
    "description": "string",
    "dueDate": "2026-04-02T19:16:18.864Z",
    "createdAt": "2026-04-02T19:16:18.864Z",
    "updatedAt": "2026-04-02T19:16:18.864Z",
    "status": 0,
    "completedAt": "2026-04-02T19:16:18.864Z",
    "canAnyoneJoin": true,
    "ownerId": "string",
    "ownerUsername": "string",
    "performers": [
      {
        "id": "string",
        "name": "string",
        "username": "string"
      }
    ]
  }
  ```

- `POST /tasks/join/{id}`

  **Description:** join a task if it is open

  **Roles:**
  - User

  **Response codes:**
  - 200 OK
  - 400 Bad Request
  - 401 Unauthorized
  - 403 Forbidden
  - 404 Not Found

  **Responses:**

  ```json
  "message": "You have successfully joined the task."
  ```

- `POST /tasks/join-request/{id}`

  **Description:** Create a request to join a task if it is closed

  **Roles:**
  - User

  **Response codes:**
  - 200 OK
  - 400 Bad Request
  - 401 Unauthorized
  - 403 Forbidden
  - 404 Not Found

  **Request body:**

  ```json
  {
    "description": "stringstringstringstringstringstringstringstringst"
  }
  ```

  **Responses:**

  ```json
  "message": "Your request to join the task has been sent to the owner."
  ```

- `DELETE /tasks/delete/{id}`

  **Description:** Delete your task

  **Roles:**
  - Employer

  **Response codes:**
  - 204 No Content
  - 401 Unauthorized
  - 403 Forbidden
  - 404 Not Found

  **Responses:**
  - 204 No Content - empty response body

  ### User

- `GET /users`

  **Description:** get list of users

  **Roles:**
  - Admin

  **Response codes:**
  - 200 Ok
  - 401 Unauthorized
  - 403 Forbidden

  **Responses:**

  ```json
  [
    {
      "id": "string",
      "name": "string",
      "nickname": "string",
      "age": 100,
      "createdAt": "2026-04-02T20:06:29.201Z",
      "ownerTasks": [
        {
          "id": 0,
          "title": "string",
          "status": 0
        }
      ],
      "performerTasks": [
        {
          "id": 0,
          "title": "string",
          "status": 0
        }
      ]
    }
  ]
  ```

- `GET /users/{id}`

  **Description:** get a user by ID

  **Roles:**
  - Admin

  **Response codes:**
  - 200 Ok
  - 401 Unauthorized
  - 403 Forbidden
  - 404 Not Found

  **Responses:**

  ```json
  {
    "id": "string",
    "name": "string",
    "nickname": "string",
    "age": 100,
    "createdAt": "2026-04-02T20:07:15.990Z",
    "ownerTasks": [
      {
        "id": 0,
        "title": "string",
        "status": 0
      }
    ],
    "performerTasks": [
      {
        "id": 0,
        "title": "string",
        "status": 0
      }
    ]
  }
  ```

- `PATCH /users/{id}`

  **Description:** change user by ID

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
    "name": "string",
    "nickname": "string",
    "age": 0
  }
  ```

  **Responses:**

  ```json
  "message": "Data of user {id} updated"
  ```

- `DELETE /users/{id}`

  **Description:** delete user by ID

  **Roles:**
  - Admin

  **Response codes:**
  - 204 No Content
  - 401 Unauthorized
  - 403 Forbidden
  - 404 Not Found

  **Responses:**
  - 204 No Content - empty response body
