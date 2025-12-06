# CodeLeap Docker Deployment Guide

## Quick Start

### 1. Build and Run
```bash
docker-compose up -d --build
```

### 2. View Logs
```bash
# API logs
docker-compose logs -f api
```

### 3. Stop Service
```bash
docker-compose down
```

---

## Services

### 🚀 API Service
- **URL**: http://localhost:8080
- **Swagger**: http://localhost:8080/swagger
- **Container**: codeleap-api
- **Auto-migrates** AWS RDS database on startup
- **Connects to**: AWS RDS PostgreSQL (stockpos.c18eoyyecdq1.ap-southeast-1.rds.amazonaws.com)

---

## Environment Variables

The `docker-compose.yml` includes:

- ✅ AWS RDS PostgreSQL connection string
- ✅ JWT configuration (Access/Refresh keys, Issuer, Audience)
- ✅ Token expiry settings

**Database**: Uses your existing AWS RDS PostgreSQL
- Host: `stockpos.c18eoyyecdq1.ap-southeast-1.rds.amazonaws.com`
- Database: `codeleap_postgres_test`
- Username: `stockposadmin`

---

## Development vs Production

### Development Mode
Create `docker-compose.override.yml`:
```yaml
version: '3.8'

services:
  api:
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
    volumes:
      - ./src:/src  # Hot reload
```

Then run:
```bash
docker-compose up -d
```

### Production Mode
Use the default `docker-compose.yml` as-is. The API runs in Production mode by default.

---

## Health Checks

API has health check configured:

### API Health Check
```bash
curl http://localhost:8080/swagger/index.html
```

Check service status:
```bash
docker-compose ps
```
You should see `healthy` status for the API.

---

## Database Migration

The API **automatically applies migrations** on startup to your AWS RDS PostgreSQL database! 

When you start the container:
1. API connects to AWS RDS PostgreSQL
2. Runs pending migrations automatically
3. Seeds default roles (Admin, User) if they don't exist
4. Ready to accept requests!

---

## Testing the Deployment

### 1. Check Service is Running
```bash
docker-compose ps
```

Expected output:
```
NAME             STATUS          PORTS
codeleap-api    Up (healthy)    0.0.0.0:8080->8080/tcp
```

### 2. Register a User
```bash
curl -X POST http://localhost:8080/api/Auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "username": "testuser",
    "password": "Test@123"
  }'
```

### 3. Login
```bash
curl -X POST http://localhost:8080/api/Auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "username": "testuser",
    "password": "Test@123"
  }'
```

### 4. Access Swagger UI
Open your browser: http://localhost:8080/swagger

---

## Useful Commands

### Rebuild API
```bash
docker-compose up -d --build
```

### Restart API
```bash
docker-compose restart api
```

### Access API Container Shell
```bash
docker exec -it codeleap-api /bin/bash
```

### Check API Logs (Last 100 lines)
```bash
docker-compose logs --tail=100 api
```

---

## Troubleshooting

### API Won't Start
Check API logs:
```bash
docker-compose logs api
```

### Database Connection Error
1. Verify your AWS RDS PostgreSQL is accessible
2. Check security group allows connections from your IP
3. Verify connection string in `docker-compose.yml` or use environment variables

### Port Already in Use
If port 8080 is already in use, modify the port in `docker-compose.yml`:
```yaml
ports:
  - "9080:8080"  # Use port 9080 instead of 8080
```

---

## Data Persistence

All data is stored in your **AWS RDS PostgreSQL database**, providing:
- ✅ Automatic backups
- ✅ High availability
- ✅ Managed service (no manual maintenance)

The Docker container is stateless - you can recreate it anytime without data loss.

---

## Production Deployment

For production deployments:

1. **Use Environment Variables**:
   Create `.env` file:
   ```env
   POSTGRES_HOST=your-rds-endpoint.amazonaws.com
   POSTGRES_DB=your_database_name
   POSTGRES_USER=your_username
   POSTGRES_PASSWORD=your_strong_password
   JWT_ACCESS_KEY=your_generated_key
   JWT_REFRESH_KEY=your_generated_key
   ```

   Then update `docker-compose.yml`:
   ```yaml
   environment:
     - ConnectionStrings__aws_postgres_url=HOST=${POSTGRES_HOST};Port=5432;Database=${POSTGRES_DB};Username=${POSTGRES_USER};Password=${POSTGRES_PASSWORD}
     - Jwt__Access_Key=${JWT_ACCESS_KEY}
     - Jwt__Refresh_Key=${JWT_REFRESH_KEY}
   ```

2. **Enable HTTPS**:
   Add reverse proxy (nginx/traefik/AWS ALB) in front of the API

3. **Use AWS ECR/ECS**:
   Push Docker image to AWS ECR and deploy via ECS for better integration

4. **Environment-specific Settings**:
   Use different RDS instances for dev/staging/production

---

## Summary

✅ **One command deployment**: `docker-compose up -d --build`  
✅ **Auto-migration on startup**  
✅ **Connects to AWS RDS PostgreSQL**  
✅ **Health checks included**  
✅ **Stateless container** - easy to scale and redeploy  

Your ASP.NET Core API with Microsoft Identity is now containerized and ready to deploy with AWS RDS! 🚀

