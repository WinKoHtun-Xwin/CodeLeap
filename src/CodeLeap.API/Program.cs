using CodeLeap.API.Middleware;
using CodeLeap.API.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;

namespace CodeLeap.API
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers(options =>
            {
                options.Filters.Add<ModelValidationFilterAttribute>();
            });

            builder.Services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            });

            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddSwaggerGen(c =>
            {
                const string bearerScheme = "Bearer";
                
                c.SwaggerDoc("v1", new OpenApiInfo 
                { 
                    Title = "CodeLeap API", 
                    Version = "v1.0.0",
                    Description = @"
                    # CodeLeap API Documentation

                    A comprehensive ASP.NET Core Web API for managing users and products with robust authentication and authorization.

                    ## Features
                    - JWT-based authentication with refresh tokens
                    - Role-based authorization (Admin, User)
                    - CRUD operations for Users and Products
                    - Comprehensive error handling and logging
                    - Clean Architecture implementation

                    ## Authentication Flow
                    1. **Register**: Create a new user account at `/api/Auth/register`
                    2. **Login**: Authenticate with credentials at `/api/Auth/login` to receive JWT tokens
                    3. **Access Protected Endpoints**: Include the JWT token in the Authorization header
                    4. **Refresh Token**: Use the refresh token at `/api/Auth/refreshToken/{refreshToken}` to get new access tokens

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

                c.AddSecurityDefinition(bearerScheme, new OpenApiSecurityScheme
                {
                    Description = @"JWT Authorization header using the Bearer scheme.
                      
Enter 'Bearer' [space] and then your token in the text input below.
                      
Example: 'Bearer 12345abcdef'",
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
                                Id = bearerScheme
                            },
                            Scheme = "oauth2",
                            Name = bearerScheme,
                            In = ParameterLocation.Header,
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

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Access_Key"]!)),
                    ClockSkew = TimeSpan.Zero
                };

                options.Events = new JwtBearerEvents
                {
                    OnChallenge = async context =>
                    {
                        context.HandleResponse();
                        context.Response.StatusCode = 401;
                        context.Response.ContentType = "application/json";

                        var response = CodeLeap.Application.Common.BaseResponseModel<object>.Failure(
                            CodeLeap.Application.Common.ResponseMessage.GeneralMessage.Unauthorized,
                            "Authentication token is missing or invalid. Please provide a valid Bearer token in the Authorization header."
                        );

                        var jsonResponse = System.Text.Json.JsonSerializer.Serialize(response, new System.Text.Json.JsonSerializerOptions
                        {
                            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
                        });

                        await context.Response.WriteAsync(jsonResponse);
                    }
                };
            });

            builder.Services.AddAuthorization(options =>
            {
                options.FallbackPolicy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();

                options.AddPolicy("AdminOnly", policy => 
                    policy.RequireRole("Admin"));
                
                options.AddPolicy("UserOrAdmin", policy => 
                    policy.RequireRole("User", "Admin"));
                
                options.AddPolicy("AuthenticatedUser", policy => 
                    policy.RequireAuthenticatedUser());
            });

            // Add HttpContextAccessor
            builder.Services.AddHttpContextAccessor();

            builder.Services.AddApiDI(builder.Configuration);

            // Add logging (built-in)
            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();
            builder.Logging.AddDebug();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

            app.UseHttpsRedirection();

            app.UseAuthentication();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
