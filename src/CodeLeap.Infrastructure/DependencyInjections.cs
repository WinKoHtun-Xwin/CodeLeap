using CodeLeap.Application.Interfaces;
using CodeLeap.Core.IRepositories;
using CodeLeap.Infrastructure.PostgresSQL;
using CodeLeap.Infrastructure.Security;
using CodeLeap.Infrastructure.Repositories;
using CodeLeap.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CodeLeap.Infrastructure
{
    public static class DependencyInjections
    {
        public static IServiceCollection AddInfrastructureDI(this IServiceCollection services,IConfiguration configuration)
        {
            services.AddDbContext<PostgresSqlDbContext>(Options =>
            {
                Options.UseNpgsql(configuration.GetConnectionString("aws_postgres_url"), 
                    b => b.MigrationsAssembly("CodeLeap.Infrastructure"));
            });

            // Note: Identity services are configured in Program.cs

            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
            
            services.AddScoped<IJwtService, JwtService>();
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            
            // IPasswordService is no longer needed - Identity UserManager handles password hashing
            // services.AddTransient<IPasswordService, PasswordService>(); // REMOVED
            
            services.AddSingleton(typeof(ILoggerService<>), typeof(LoggerService<>));
            
            return services;
        }
    }
}
