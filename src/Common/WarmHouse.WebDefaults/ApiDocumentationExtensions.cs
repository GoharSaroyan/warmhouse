using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace WarmHouse.WebDefaults;

/// <summary>
/// One shared way to wire up Swagger/OpenAPI, used by every microservice
/// (Task 4, section 2) so each one doesn't hand-roll its own setup. Every
/// service gets a Swagger UI at /swagger and the raw document at
/// /swagger/v1/swagger.json - reachable directly, and through the API
/// Gateway too, since the gateway forwards whatever path it doesn't
/// strip a prefix from.
/// </summary>
public static class ApiDocumentationExtensions
{
    public static WebApplicationBuilder AddApiDocumentation(this WebApplicationBuilder builder, string title, string description)
    {
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = title,
                Version = "v1",
                Description = description,
            });
        });

        return builder;
    }

    public static WebApplication UseApiDocumentation(this WebApplication app, string title)
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("v1/swagger.json", $"{title} v1");
        });

        return app;
    }
}
