using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;

namespace WarmHouse.WebDefaults;

/// <summary>
/// One shared way to wire up Swagger/OpenAPI, used by every microservice
/// (Task 4, section 2) so each one doesn't hand-roll its own setup. Every
/// service gets a Swagger UI at /swagger and the raw document at
/// /swagger/v1/swagger.json - reachable directly, and through the API
/// Gateway too, since the gateway forwards whatever path it doesn't
/// strip a prefix from.
///
/// Beyond bare endpoint discovery, this also wires up:
/// - XML doc comments (&lt;summary&gt;/&lt;remarks&gt;/&lt;param&gt;) as
///   endpoint/parameter descriptions - requires
///   &lt;GenerateDocumentationFile&gt; in the calling project.
/// - [ProducesResponseType] response codes per action.
/// - Request/response examples via Swashbuckle.AspNetCore.Filters
///   ([SwaggerRequestExample]/[SwaggerResponseExample]).
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

            // Picks up <summary>/<remarks>/<param> from the calling
            // service's own XML doc file, if it generated one.
            var xmlFile = $"{Assembly.GetEntryAssembly()!.GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
            }

            options.ExampleFilters();
        });

        builder.Services.AddSwaggerExamplesFromAssemblies(Assembly.GetEntryAssembly()!);

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
