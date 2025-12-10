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
        public static IServiceCollection AddInfrastructureDI(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<PostgresSqlDbContext>(Options =>
            {
                Options.UseNpgsql(configuration.GetConnectionString("aws_postgres_url"),
                    b => b.MigrationsAssembly("CodeLeap.Infrastructure"));
            });

            services.AddScoped<IProductRepository, ProductRepository>();


            services.AddScoped<IKeycloakUserinfoService, KeycloakUserinfoService>();

            services.AddScoped<IPasswordService, PasswordService>();

            services.AddSingleton(typeof(ILoggerService<>), typeof(LoggerService<>));

            return services;
        }
    }
}
