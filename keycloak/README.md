# Keycloak Configuration Guide

## Overview

This directory contains the Keycloak realm configuration for the CodeLeap API. The realm is automatically imported when Keycloak starts via docker-compose.

## Accessing Keycloak Admin Console

1. Start the services:
   ```bash
   docker-compose up -d
   ```

2. Access the admin console at: **http://localhost:8080**

3. Login with default credentials:
   - **Username**: `admin`
   - **Password**: `admin`

## Pre-configured Realm: CodeLeap

### Client Configuration

- **Client ID**: `codeleap-api`
- **Client Secret**: `codeleap-api-secret`
- **Access Type**: Confidential
- **Direct Access Grants**: Enabled (for password grant flow)
- **Service Accounts**: Enabled

### Roles

Two realm roles are pre-configured:
- **Admin**: Full administrative access
- **User**: Standard user access

### Test Users

#### Admin User
- **Username**: `admin`
- **Password**: `admin123`
- **Email**: `admin@codeleap.com`
- **Roles**: Admin, User

#### Standard User
- **Username**: `testuser`
- **Password**: `user123`
- **Email**: `user@codeleap.com`
- **Roles**: User

## Obtaining Access Tokens

### Using Password Grant (Direct Access)

```bash
curl -X POST 'http://localhost:8080/realms/CodeLeap/protocol/openid-connect/token' \
  -H 'Content-Type: application/x-www-form-urlencoded' \
  -d 'client_id=codeleap-api' \
  -d 'client_secret=codeleap-api-secret' \
  -d 'username=admin' \
  -d 'password=admin123' \
  -d 'grant_type=password'
```

Response includes:
- `access_token`: JWT token to use for API authentication
- `refresh_token`: Token to refresh the access token
- `expires_in`: Token expiration time (900 seconds = 15 minutes)

### Using the Access Token

```bash
# Extract the token from the response
ACCESS_TOKEN="<your-access-token>"

# Call protected API endpoint
curl -X GET 'http://localhost:5000/api/User/me' \
  -H "Authorization: Bearer $ACCESS_TOKEN"
```

## Token Configuration

- **Access Token Lifespan**: 15 minutes (900 seconds)
- **SSO Session Idle**: 30 minutes
- **SSO Session Max**: 10 hours
- **Refresh Token**: Sliding window, updates on each use

## Managing Users

### Create New User (via Admin Console)

1. Navigate to **Users** in the CodeLeap realm
2. Click **Add user**
3. Fill in user details (username, email, etc.)
4. Click **Save**
5. Go to **Credentials** tab
6. Set password (uncheck "Temporary")
7. Go to **Role Mappings** tab
8. Assign appropriate roles (User, Admin, or both)

### Assign Roles to Users

1. Select user from Users list
2. Go to **Role Mappings** tab
3. Select roles from **Available Roles**
4. Click **Add selected**

## Troubleshooting

### Keycloak not accessible

Check if the service is running:
```bash
docker-compose ps keycloak
docker-compose logs keycloak
```

### Import Failed

If realm import fails, manually import:
1. Access Keycloak admin console
2. Hover over realm dropdown (top left)
3. Click **Create Realm**
4. Upload `realm-export.json`

### Token Validation Errors

Ensure the API is configured with correct Keycloak URL:
- Container-to-container: `http://keycloak:8080`
- Host machine: `http://localhost:8080`

Check `appsettings.Docker.json` for correct configuration.

## Security Notes

> [!WARNING]
> **Production Configuration Required**
> 
> The default configuration uses:
> - **SSL disabled** (`ssl-required: none`)
> - **Default passwords** (admin/admin, admin123, user123)
> - **Simple client secret** (codeleap-api-secret)
> 
> For production:
> 1. Enable SSL/TLS (`ssl-required: external` or `all`)
> 2. Change admin password immediately
> 3. Generate strong client secrets
> 4. Enable brute force protection (already configured)
> 5. Consider using certificate-based authentication
> 6. Review and restrict CORS origins

## Advanced Configuration

### Custom Token Claims

Protocol mappers are configured to include:
- `preferred_username`: User's username
- `email`: User's email address
- `roles`: Array of user's realm roles

### Adding Custom Claims

1. Go to **Clients** → `codeleap-api` → **Client scopes**
2. Select a scope or create new
3. Go to **Mappers** tab
4. Click **Create** to add custom mapper
5. Configure claim name and value source

### Federation with External Identity Providers

Keycloak supports federation with:
- LDAP / Active Directory
- Social providers (Google, Facebook, GitHub, etc.)
- SAML / OpenID Connect identity providers

Configure in: **Identity Providers** section of the realm.

## API Integration Endpoints

### Well-known Configuration
```
http://localhost:8080/realms/CodeLeap/.well-known/openid-configuration
```

### Token Endpoint
```
http://localhost:8080/realms/CodeLeap/protocol/openid-connect/token
```

### User Info Endpoint
```
http://localhost:8080/realms/CodeLeap/protocol/openid-connect/userinfo
```

### Logout Endpoint
```
http://localhost:8080/realms/CodeLeap/protocol/openid-connect/logout
```
