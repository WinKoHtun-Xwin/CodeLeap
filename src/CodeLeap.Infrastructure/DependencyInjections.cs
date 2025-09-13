using CodeLeap.Core.IRepositories;
using CodeLeap.Infrastructure.PostgresSQL;
using CodeLeap.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeLeap.Infrastructure
{
    public static class DependencyInjections
    {
        public static IServiceCollection AddInfrastructureDI(this IServiceCollection services,IConfiguration configuration)
        {
            services.AddDbContext<PostgresSQLDbContext>(Options =>
            {
                Options.UseNpgsql(configuration.GetConnectionString("aws_postgres_url"), 
                    b => b.MigrationsAssembly("CodeLeap.Infrastructure"));
            });

            services.AddScoped<IUserRepository, UserRepository>();
            return services;
        }
    }
}
