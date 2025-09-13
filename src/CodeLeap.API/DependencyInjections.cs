using CodeLeap.Application;
using CodeLeap.Infrastructure;

namespace CodeLeap.API
{
    public static class DependencyInjections
    {
        public static IServiceCollection AddApiDI(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddApplicationDI();
            services.AddInfrastructureDI(configuration);
            return services;
        }
    }
}
