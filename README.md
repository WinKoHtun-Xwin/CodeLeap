# CodeLeap Technical Test API

A comprehensive ASP.NET Core Web API built with Clean Architecture principles, featuring JWT authentication, role-based authorization, and CRUD operations for user and product management.

## 🚀 Features

- **Clean Architecture** - Separation of concerns with distinct layers
- **JWT Authentication** - Secure token-based authentication with refresh tokens
- **Role-Based Authorization** - Admin and User roles with policy-based access control
- **RESTful API** - Full CRUD operations for Users and Products
- **PostgreSQL Database** - Entity Framework Core with Code-First migrations
- **Comprehensive Documentation** - OpenAPI/Swagger documentation with detailed endpoint information
- **Global Error Handling** - Centralized exception handling middleware
- **Model Validation** - Automatic request validation with detailed error responses
- **Docker Support** - Containerized deployment ready

## 🏗️ Architecture Overview

The project follows Clean Architecture principles with the following layers:

```
┌─────────────────┐    ┌─────────────────┐
│   Presentation  │────│      API        │
│   (Controllers) │    │   (CodeLeap.API)│
└─────────────────┘    └─────────────────┘
         │                       │
         ▼                       ▼
┌─────────────────┐    ┌─────────────────┐
│   Application   │    │   Infrastructure│
│(Business Logic) │    │  (Data Access)  │
│CodeLeap.App     │    │CodeLeap.Infra   │
└─────────────────┘    └─────────────────┘
         │                       │
         ▼                       ▼
┌─────────────────────────────────────────┐
│              Domain                     │
│         (Core Entities)                 │
│        (CodeLeap.Core)                  │
└─────────────────────────────────────────┘
```

## 🛠️ Prerequisites

- **.NET 8.0 SDK** or later
- **PostgreSQL** database
- **Visual Studio 2022** or **VS Code** (optional)
- **Docker** (optional, for containerized deployment)

## ⚡ Quick Start

### 1. Clone the Repository

```bash
git clone <repository-url>
cd CodeLeap
```

### 2. Configure Database Connection

Update the connection string in `src/CodeLeap.API/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "aws_postgres_url": "Host=localhost;Port=5432;Database=codeleap_db;Username=your_username;Password=your_password"
  }
}
```

### 3. Configure JWT Settings

Update JWT configuration in `src/CodeLeap.API/appsettings.json`:

```json
{
  "Jwt": {
    "Access_Key": "your-256-bit-secret-key-here",
    "Refresh_Key": "your-256-bit-refresh-secret-key-here",
    "Issuer": "CodeLeap",
    "Audience": "CodeLeapUsers",
    "AccessTokenExpiryMinutes": 15,
    "RefreshTokenExpiryDays": 7
  }
}
```

### 4. Run Database Migrations

```bash
cd src/CodeLeap.API
dotnet ef database update
```

### 5. Run the Application

```bash
dotnet run
```

The API will be available at:
- **HTTP**: `http://localhost:5000`
- **HTTPS**: `https://localhost:5001`
- **Swagger UI**: `https://localhost:5001/swagger`

## 📋 API Documentation

### Authentication Endpoints

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| POST | `/api/Auth/register` | Register new user | No |
| POST | `/api/Auth/login` | User login | No |
| POST | `/api/Auth/refreshToken/{token}` | Refresh access token | No |

### User Management Endpoints

| Method | Endpoint | Description | Auth Required | Role |
|--------|----------|-------------|---------------|------|
| GET | `/api/User` | Get all users | Yes | Any |
| GET | `/api/User/me` | Get current user profile | Yes | Any |
| GET | `/api/User/me/info` | Get current user info & claims | Yes | Any |
| GET | `/api/User/{id}` | Get user by ID | Yes | Any |
| POST | `/api/User` | Create new user | Yes | Any |
| PUT | `/api/User/{id}` | Update user | Yes | Any |
| DELETE | `/api/User/{id}` | Delete user | Yes | Any |

### Product Management Endpoints

| Method | Endpoint | Description | Auth Required | Role |
|--------|----------|-------------|---------------|------|
| GET | `/api/Product` | Get all products | No | - |
| GET | `/api/Product/pagination` | Get products with pagination | No | - |
| GET | `/api/Product/{id}` | Get product by ID | Yes | Any |
| GET | `/api/Product/my-products` | Get current user's products | Yes | Any |
| POST | `/api/Product` | Create new product | Yes | Any |
| PUT | `/api/Product/{id}` | Update product | Yes | Any |
| DELETE | `/api/Product/{id}` | Delete product | Yes | **Admin Only** |

## 🔐 Authentication & Authorization

### JWT Token Flow

1. **Register** a new account or **Login** with existing credentials
2. Receive **Access Token** (15 min expiry) and **Refresh Token** (7 days expiry)
3. Include Access Token in Authorization header: `Bearer <token>`
4. Use Refresh Token to get new Access Token when expired

### Authorization Policies

- **AdminOnly**: Requires `Admin` role
- **UserOrAdmin**: Requires `User` or `Admin` role  
- **AuthenticatedUser**: Requires any authenticated user

### Example Authentication Flow

```bash
# 1. Register a new user
curl -X POST "https://localhost:5001/api/Auth/register" \
  -H "Content-Type: application/json" \
  -d '{"username": "john_doe", "password": "SecurePass123!"}'

# 2. Login to get tokens
curl -X POST "https://localhost:5001/api/Auth/login" \
  -H "Content-Type: application/json" \
  -d '{"username": "john_doe", "password": "SecurePass123!"}'

# 3. Use access token for protected endpoints
curl -X GET "https://localhost:5001/api/User/me" \
  -H "Authorization: Bearer <your-access-token>"
```

## 🐳 Docker Deployment

### Build Docker Image

```bash
docker build -t codeleap-api .
```

### Run with Docker

```bash
docker run -d \
  --name codeleap-api \
  -p 8080:8080 \
  -e ConnectionStrings__aws_postgres_url="Host=host.docker.internal;Port=5432;Database=codeleap_db;Username=postgres;Password=password" \
  codeleap-api
```

### Docker Compose (with PostgreSQL)

```yaml
version: '3.8'
services:
  api:
    build: .
    ports:
      - "8080:8080"
    environment:
      - ConnectionStrings__aws_postgres_url=Host=db;Port=5432;Database=codeleap_db;Username=postgres;Password=password
    depends_on:
      - db

  db:
    image: postgres:15
    environment:
      - POSTGRES_DB=codeleap_db
      - POSTGRES_USER=postgres
      - POSTGRES_PASSWORD=password
    ports:
      - "5432:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data

volumes:
  postgres_data:
```

## 🧪 Testing

### Run Unit Tests

```bash
cd src/CodeLeap.Test
dotnet test
```

### API Testing with Swagger

1. Navigate to `https://localhost:5001/swagger`
2. Use the **Authorize** button to authenticate
3. Test endpoints directly from the Swagger UI

## 📁 Project Structure

```
CodeLeap/
├── src/
│   ├── CodeLeap.API/           # Web API layer
│   │   ├── Controllers/        # API controllers
│   │   ├── Middleware/         # Custom middleware
│   │   ├── Filters/           # Action filters
│   │   └── Extensions/        # Extension methods
│   ├── CodeLeap.Application/   # Business logic layer
│   │   ├── Services/          # Business services
│   │   ├── DTOs/             # Data transfer objects
│   │   └── Interfaces/       # Service interfaces
│   ├── CodeLeap.Core/         # Domain layer
│   │   ├── Entities/         # Domain entities
│   │   └── IRepositories/    # Repository interfaces
│   └── CodeLeap.Infrastructure/ # Data access layer
│       ├── Repositories/      # Repository implementations
│       ├── Services/         # Infrastructure services
│       └── PostgresSQL/      # Database context
└── CodeLeap.Test/             # Unit tests
```

## 🔧 Configuration

### Environment Variables

| Variable | Description | Default |
|----------|-------------|---------|
| `ASPNETCORE_ENVIRONMENT` | Environment (Development/Production) | Development |
| `ConnectionStrings__aws_postgres_url` | PostgreSQL connection string | - |
| `Jwt__Access_Key` | JWT signing key | - |
| `Jwt__Issuer` | JWT issuer | CodeLeap |

### appsettings.json Structure

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ConnectionStrings": {
    "aws_postgres_url": "connection-string-here"
  },
  "Jwt": {
    "Access_Key": "secret-key",
    "Refresh_Key": "refresh-secret-key", 
    "Issuer": "CodeLeap",
    "Audience": "CodeLeapUsers",
    "AccessTokenExpiryMinutes": 15,
    "RefreshTokenExpiryDays": 7
  }
}
```

