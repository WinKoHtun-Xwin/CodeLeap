# CodeLeap Technical Test API

A comprehensive ASP.NET Core Web API built with Clean Architecture principles, featuring JWT authentication, role-based authorization, and CRUD operations for user and product management.

## 🚀 Features

- **Clean Architecture** - Separation of concerns with distinct layers
- **Keycloak Authentication** - Industry-standard OAuth2/OpenID Connect authentication
- **Role-Based Authorization** - Admin and User roles with policy-based access control
- **RESTful API** - Full CRUD operations for Users and Products
- **PostgreSQL Database** - Entity Framework Core with Code-First migrations
- **Comprehensive Documentation** - OpenAPI/Swagger documentation with detailed endpoint information
- **Global Error Handling** - Centralized exception handling middleware
- **Model Validation** - Automatic request validation with detailed error responses
- **Docker Support** - Full docker-compose configuration with Keycloak, PostgreSQL, and API

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

### Option 1: Docker Compose (Recommended)

The easiest way to get started with full Keycloak integration:

```bash
# 1. Clone the repository
git clone <repository-url>
cd CodeLeap

# 2. Start all services (Keycloak, PostgreSQL, API)
docker-compose up -d

# 3. Wait for services to be healthy (30-60 seconds)
docker-compose ps

# 4. Access the services
# - API: http://localhost:5000
# - Swagger UI: http://localhost:5000/swagger
# - Keycloak Admin: http://localhost:8080 (admin/admin)
```

**Pre-configured Test Users:**
- **Admin**: username: `admin`, password: `admin123`
- **User**: username: `testuser`, password: `user123`

### Option 2: Local Development

For local development without Docker:

#### 1. Clone the Repository

```bash
git clone <repository-url>
cd CodeLeap
```

#### 2. Start Keycloak (using Docker)

```bash
docker-compose up -d keycloak
```

Or run Keycloak separately if you prefer.

#### 3. Configure Database Connection

Update the connection string in `src/CodeLeap.API/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "aws_postgres_url": "Host=localhost;Port=5432;Database=codeleap_db;Username=your_username;Password=your_password"
  }
}
```

#### 4. Configure Keycloak Settings

Keycloak is pre-configured in `appsettings.json`:

```json
{
  "Keycloak": {
    "realm": "CodeLeap",
    "auth-server-url": "http://localhost:8080",
    "resource": "codeleap-api",
    "credentials": {
      "secret": "codeleap-api-secret"
    }
  }
}
```

#### 5. Run Database Migrations

```bash
cd src/CodeLeap.API
dotnet ef database update
```

#### 6. Run the Application

```bash
dotnet run
```

The API will be available at:
- **HTTP**: `http://localhost:5000`
- **HTTPS**: `https://localhost:5001`
- **Swagger UI**: `https://localhost:5001/swagger`

## 📋 API Documentation

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

### Keycloak Authentication

CodeLeap API uses Keycloak for all authentication via OAuth2/OpenID Connect.

#### Obtaining Tokens

**Method 1: Using Swagger UI (Easiest)**
1. Navigate to http://localhost:5000/swagger
2. Click the **Authorize** button
3. Select the `oauth2` scheme
4. Enter credentials:
   - Admin: `admin` / `admin123`
   - User: `testuser` / `user123`
5. Tokens are automatically managed

**Method 2: Direct API Call**

```bash
curl -X POST 'http://localhost:8080/realms/CodeLeap/protocol/openid-connect/token' \
  -H 'Content-Type: application/x-www-form-urlencoded' \
  -d 'client_id=codeleap-api' \
  -d 'client_secret=codeleap-api-secret' \
  -d 'username=admin' \
  -d 'password=admin123' \
  -d 'grant_type=password'
```

Response:
```json
{
  "access_token": "eyJhbGciOiJSUzI1NiIsInR5cCI...",
  "expires_in": 900,
  "refresh_expires_in": 1800,
  "refresh_token": "eyJhbGciOiJIUzI1NiIsInR5cCI...",
  "token_type": "Bearer"
}
```

#### Using the Access Token

```bash
# Use the access token in the Authorization header
curl -X GET 'http://localhost:5000/api/User/me' \
  -H 'Authorization: Bearer eyJhbGciOiJSUzI1NiIsInR5cCI...'
```

### Authorization Policies

- **AdminOnly**: Requires `Admin` role
- **UserOrAdmin**: Requires `User` or `Admin` role  
- **AuthenticatedUser**: Requires any authenticated user

### Keycloak Admin Console

Access the Keycloak admin console to manage users, roles, and clients:

- **URL**: http://localhost:8080
- **Username**: `admin`
- **Password**: `admin`
- **Realm**: `CodeLeap`

For detailed Keycloak configuration, see [`keycloak/README.md`](keycloak/README.md).

## 🐳 Docker Deployment

### Docker Compose (Recommended)

The project includes a complete docker-compose setup with:
- Keycloak authentication server
- PostgreSQL database for Keycloak
- PostgreSQL database for CodeLeap application
- CodeLeap API

```bash
# Start all services
docker-compose up -d

# View logs
docker-compose logs -f

# Stop all services
docker-compose down

# Stop and remove volumes (clean slate)
docker-compose down -v
```

### Service URLs

- **API**: http://localhost:5000
- **Swagger**: http://localhost:5000/swagger
- **Keycloak**: http://localhost:8080
- **PostgreSQL (Keycloak)**: localhost:5432
- **PostgreSQL (CodeLeap)**: localhost:5433

### Environment Variables

Copy `.env.example` to `.env` and customize:

```bash
cp .env.example .env
```

### Docker Compose Services

```yaml
services:
  keycloak:        # Authentication server
  keycloak-db:     # Keycloak PostgreSQL
  codeleap-api:    # API service
  codeleap-db:     # Application PostgreSQL
```

### Health Checks

All services include health checks. View status:

```bash
docker-compose ps
```

### Manual Docker Build

If you want to build the API image manually:

```bash
# Build the API image
docker build -t codeleap-api -f src/CodeLeap.API/Dockerfile .

# Run the container
docker run -d \
  --name codeleap-api \
  -p 5000:80 \
  -e ConnectionStrings__aws_postgres_url="Host=host.docker.internal;Port=5432;..." \
  -e Keycloak__auth-server-url="http://host.docker.internal:8080" \
  codeleap-api
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

