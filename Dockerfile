# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["src/CodeLeap.API/CodeLeap.API.csproj", "CodeLeap.API/"]
COPY ["src/CodeLeap.Application/CodeLeap.Application.csproj", "CodeLeap.Application/"]
COPY ["src/CodeLeap.Core/CodeLeap.Core.csproj", "CodeLeap.Core/"]
COPY ["src/CodeLeap.Infrastructure/CodeLeap.Infrastructure.csproj", "CodeLeap.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "CodeLeap.API/CodeLeap.API.csproj"

# Copy all source code
COPY src/ .

# Build the application
WORKDIR "/src/CodeLeap.API"
RUN dotnet build "CodeLeap.API.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "CodeLeap.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

COPY --from=publish /app/publish .

# Expose ports
EXPOSE 8080
EXPOSE 8081

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "CodeLeap.API.dll"]
