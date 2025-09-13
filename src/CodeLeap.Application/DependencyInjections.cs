using CodeLeap.Application.Interfaces;
using CodeLeap.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CodeLeap.Application
{
    public static class DependencyInjections
    {
        public static IServiceCollection AddApplicationDI(this IServiceCollection services)
        {
            // Register services
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IProductService, ProductService>();

            return services;
        }
    }
}
