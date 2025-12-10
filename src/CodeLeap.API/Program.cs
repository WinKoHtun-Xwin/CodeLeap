using CodeLeap.API.Middleware;
using CodeLeap.API.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using CodeLeap.Application.Common;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.Text.Json;

namespace CodeLeap.API
{
    public static class Program
    {
        // Cached JsonSerializerOptions to avoid creating new instances for each serialization
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers(options =>
            {
                options.Filters.Add<ModelValidationFilterAttribute>();
                options.Filters.Add<UserAuthorizationFilter>();
            });

            builder.Services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            });

            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddSwaggerGen(c =>
            {
                const string bearerScheme = "Bearer";
                const string oauth2Scheme = "oauth2";

                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "CodeLeap API",
                    Version = "v1.0.0",
                    Description = @"
                    # CodeLeap API Documentation

                    A comprehensive ASP.NET Core Web API for managing users and products with Keycloak authentication.

                    ## Features
                    - **Keycloak Authentication** with OAuth2/OpenID Connect
                    - Role-based authorization (Admin, User)
                    - CRUD operations for Users and Products
                    - Comprehensive error handling and logging
                    - Clean Architecture implementation

                    ## Authentication Flow
                    
                    ### Option 1: Keycloak OAuth2 via Swagger (Easiest)
                    1. Click **Authorize** button below
                    2. Select 'oauth2' scheme
                    3. Login with Keycloak credentials:
                       - Admin: `admin` / `admin123`
                       - User: `testuser` / `user123`
                    4. Tokens are automatically managed
                    
                    ### Option 2: Direct Token via curl
                    1. Obtain token from Keycloak:
                       ```bash
                       curl -X POST 'http://localhost:8080/realms/CodeLeap/protocol/openid-connect/token' \
                         -d 'client_id=codeleap-api' \
                         -d 'client_secret=codeleap-api-secret' \
                         -d 'username=admin' \
                         -d 'password=admin123' \
                         -d 'grant_type=password' \
                         -d 'scope=openid'
                       ```
                    2. Copy the `access_token` from response
                    3. Click **Authorize**, select 'Bearer' scheme
                    4. Paste token (without 'Bearer' prefix)

                    ## Authorization Policies
                    - **AdminOnly**: Requires Admin role (e.g., delete operations)
                    - **UserOrAdmin**: Requires User or Admin role
                    - **AuthenticatedUser**: Requires any authenticated user

                    ## Response Format
                    All API responses follow a consistent format:
                    ```json
                    {
                    ""success"": true,
                    ""message"": ""Operation completed successfully"",
                    ""data"": { /* response data */ },
                    ""errors"": []
                    }
                    ```",
                    Contact = new OpenApiContact
                    {
                        Name = "CodeLeap Development Team",
                        Email = "dev@codeleap.com"
                    },
                    License = new OpenApiLicense
                    {
                        Name = "MIT License"
                    }
                });

                // Include XML comments
                var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (File.Exists(xmlPath))
                {
                    c.IncludeXmlComments(xmlPath);
                }

                // OAuth2 - Keycloak Integration
                var keycloakUrl = builder.Configuration["Keycloak:auth-server-url"] ?? "http://localhost:8080";
                var realm = builder.Configuration["Keycloak:realm"] ?? "CodeLeap";

                c.AddSecurityDefinition(oauth2Scheme, new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.OAuth2,
                    Flows = new OpenApiOAuthFlows
                    {
                        Password = new OpenApiOAuthFlow
                        {
                            TokenUrl = new Uri($"{keycloakUrl}/realms/{realm}/protocol/openid-connect/token"),
                            Scopes = new Dictionary<string, string>
                            {
                                { "openid", "OpenID Connect scope" },
                                { "profile", "Profile information" },
                                { "email", "Email address" }
                            }
                        },
                        AuthorizationCode = new OpenApiOAuthFlow
                        {
                            AuthorizationUrl = new Uri($"{keycloakUrl}/realms/{realm}/protocol/openid-connect/auth"),
                            TokenUrl = new Uri($"{keycloakUrl}/realms/{realm}/protocol/openid-connect/token"),
                            Scopes = new Dictionary<string, string>
                            {
                                { "openid", "OpenID Connect scope" },
                                { "profile", "Profile information" },
                                { "email", "Email address" }
                            }
                        }
                    }
                });

                // Bearer Token - Direct JWT
                c.AddSecurityDefinition(bearerScheme, new OpenApiSecurityScheme
                {
                    Description = @"JWT Authorization header using the Bearer scheme.
                      
Enter your token in the text input below (without 'Bearer' prefix).
                      
Example: 'eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9...'",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = oauth2Scheme
                            }
                        },
                        new List<string> { "openid", "profile", "email" }
                    },
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = bearerScheme
                            }
                        },
                        new List<string>()
                    }
                });

                // Add operation filters for better documentation
                c.EnableAnnotations();
                c.DescribeAllParametersInCamelCase();

                // Add examples for common responses
                c.SwaggerGeneratorOptions.DescribeAllParametersInCamelCase = true;
            });

            // Add Keycloak Authentication Services
            // This automatically registers JWT Bearer authentication for Keycloak
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    var keycloakUrl = builder.Configuration["Keycloak:auth-server-url"] ?? "http://localhost:8080";
                    var realm = builder.Configuration["Keycloak:realm"] ?? "CodeLeap";

                    options.Authority = $"{keycloakUrl}/realms/{realm}";
                    options.MetadataAddress = $"{keycloakUrl}/realms/{realm}/.well-known/openid-configuration";
                    options.RequireHttpsMetadata = builder.Configuration.GetValue<string>("Keycloak:ssl-required") != "none";
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateAudience = builder.Configuration.GetValue<bool>("Keycloak:verify-token-audience"),
                        ValidAudience = builder.Configuration["Keycloak:resource"],
                        ValidateIssuer = true,
                        ValidIssuers = [options.Authority, "http://localhost:8080/realms/CodeLeap"]
                    };

                    IConfigurationRetriever<OpenIdConnectConfiguration> configRetriever;
                    var isRunningInContainer = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";

                    if (isRunningInContainer)
                    {
                        Console.WriteLine("Running in Docker container. Using DockerInternalOidcRetriever.");
                        configRetriever = new DockerInternalOidcRetriever();
                    }
                    else
                    {
                        Console.WriteLine("Running locally. Using OpenIdConnectConfigurationRetriever.");
                        configRetriever = new OpenIdConnectConfigurationRetriever();
                    }

                    options.ConfigurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                        options.MetadataAddress,
                        configRetriever,
                        (IDocumentRetriever)new HttpDocumentRetriever { RequireHttps = options.RequireHttpsMetadata }
                    );
                    options.Events = new JwtBearerEvents
                    {
                        OnChallenge = context =>
                        {
                            context.HandleResponse();
                            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                            context.Response.ContentType = "application/json";

                            var response = BaseResponseModel<object>.Failure(
                                "Unauthorized",
                                context.ErrorDescription ?? "Authentication failed");

                            var json = JsonSerializer.Serialize(response, JsonOptions);

                            return context.Response.WriteAsync(json);
                        },
                        OnForbidden = context =>
                        {
                            context.Response.StatusCode = StatusCodes.Status403Forbidden;
                            context.Response.ContentType = "application/json";

                            var response = BaseResponseModel<object>.Failure(
                                "Forbidden",
                                "You do not have permission to access this resource");

                            var json = JsonSerializer.Serialize(response, JsonOptions);

                            return context.Response.WriteAsync(json);
                        }
                    };
                });


            builder.Services.AddAuthorizationBuilder()
                .AddPolicy("AdminOnly", policy =>
                    policy.RequireRole("Admin"))
                .AddPolicy("UserOrAdmin", policy =>
                    policy.RequireRole("User", "Admin"))
                .AddPolicy("AuthenticatedUser", policy =>
                    policy.RequireAuthenticatedUser());

            // Add HttpContextAccessor
            builder.Services.AddHttpContextAccessor();

            builder.Services.AddApiDI(builder.Configuration);

            // Add logging (built-in)
            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();
            builder.Logging.AddDebug();

            var app = builder.Build();

            // Enable Swagger in all environments (Development, Docker, Production)
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "CodeLeap API v1");
                c.OAuthClientId(builder.Configuration["Keycloak:resource"] ?? "codeleap-api");
                c.OAuthClientSecret(builder.Configuration["Keycloak:credentials:secret"] ?? "codeleap-api-secret");
                c.OAuthUsePkce();
            });

            app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

            app.UseHttpsRedirection();

            app.UseAuthentication();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }

    public class DockerInternalOidcRetriever : IConfigurationRetriever<OpenIdConnectConfiguration>
    {
        public async Task<OpenIdConnectConfiguration> GetConfigurationAsync(string address, IDocumentRetriever retriever, CancellationToken cancel)
        {
            // 1. Get the OpenID Connect discovery document (metadata)
            string doc = await retriever.GetDocumentAsync(address, cancel);

            // 2. Parse the configuration
            var config = OpenIdConnectConfiguration.Create(doc);

            // 3. Fix the JWKS URI if we are running in a container
            if (config.JwksUri != null && config.JwksUri.Contains("localhost:8080"))
            {
                Console.WriteLine($"[OIDC] Rewriting JWKS URI from '{config.JwksUri}' to internal Docker service...");
                config.JwksUri = config.JwksUri.Replace("localhost:8080", "keycloak:8080");
                Console.WriteLine($"[OIDC] New JWKS URI: '{config.JwksUri}'");
            }
            else
            {
                Console.WriteLine($"[OIDC] Using JWKS URI: '{config.JwksUri}'");
            }

            // 4. Fetch the keys using the (potentially fixed) JWKS URI
            if (!string.IsNullOrEmpty(config.JwksUri))
            {
                try
                {
                    string keys = await retriever.GetDocumentAsync(config.JwksUri, cancel);
                    config.JsonWebKeySet = new JsonWebKeySet(keys);

                    foreach (var key in config.JsonWebKeySet.Keys)
                    {
                        config.SigningKeys.Add(key);
                    }
                    Console.WriteLine($"[OIDC] Successfully loaded {config.SigningKeys.Count} signing keys.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[OIDC] Error fetching keys from {config.JwksUri}: {ex.Message}");
                    throw;
                }
            }

            return config;
        }
    }
}
