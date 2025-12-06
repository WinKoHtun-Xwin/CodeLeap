# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["src/CodeLeap.API/CodeLeap.API.csproj", "CodeLeap.API/"]
COPY ["src/CodeLeap.Application/CodeLeap.Application.csproj", "CodeLeap.Application/"]
COPY ["src/CodeLeap.Core/CodeLeap.Core.csproj", "CodeLeap.Core/"]
COPY ["src/CodeLeap.Infrastructure/CodeLeap.Infrastructure.csproj", "CodeLeap.Infrastructure/"]
COPY ["src/CodeLeap.Test/CodeLeap.Test.csproj", "CodeLeap.Test/"]

RUN dotnet restore "CodeLeap.API/CodeLeap.API.csproj"
RUN dotnet restore "CodeLeap.Test/CodeLeap.Test.csproj"

# Copy source code
COPY src/ .

# Build the API
WORKDIR /src/CodeLeap.API
RUN dotnet build "CodeLeap.API.csproj" -c Release -o /app/build

# Test stage - Run all tests before publishing
FROM build AS test
WORKDIR /src/CodeLeap.Test
RUN echo "Running tests..." && \
    dotnet test --no-restore --verbosity normal && \
    echo "All tests passed!"

# Publish stage - Only runs if tests pass
FROM build AS publish
WORKDIR /src/CodeLeap.API
RUN dotnet publish "CodeLeap.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Copy published app
COPY --from=publish /app/publish .

# Expose ports
EXPOSE 8080 8081

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Set entry point
ENTRYPOINT ["dotnet", "CodeLeap.API.dll"]
