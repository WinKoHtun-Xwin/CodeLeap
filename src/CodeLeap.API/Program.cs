
using CodeLeap.Application;
using CodeLeap.Infrastructure;
using CodeLeap.API.Middleware;
using CodeLeap.API.Filters;
using Microsoft.AspNetCore.Mvc;

namespace CodeLeap.API
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers(options =>
            {
                // Add global model validation filter
                options.Filters.Add<ModelValidationFilterAttribute>();
            });

            // Configure API behavior for model validation
            builder.Services.Configure<ApiBehaviorOptions>(options =>
            {
                // Disable default model validation response to use our custom filter
                options.SuppressModelStateInvalidFilter = true;
            });

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Custom Dependency Injections
            builder.Services.AddApiDI(builder.Configuration);

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // Add global exception handler middleware
            app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

            app.UseHttpsRedirection();

            app.UseAuthentication();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
