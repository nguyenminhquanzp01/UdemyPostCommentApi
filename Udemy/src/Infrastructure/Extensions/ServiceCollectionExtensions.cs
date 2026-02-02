namespace Udemy.Infrastructure.Extensions;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;

/// <summary>
/// Extension methods for service collection configuration.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Swagger generation with JWT Bearer authentication support and API versioning.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddSwaggerGenWithAuth(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSwaggerGen(c =>
        {
            // Add documentation for API v1.0
            c.SwaggerDoc("v1.0", new OpenApiInfo
            {
                Title = "PostComment API",
                Version = "v1.0",
                Description = "API for managing posts and comments with authentication (Initial Release)",
                Contact = new OpenApiContact
                {
                    Name = "Udemy API Support"
                },
                License = new OpenApiLicense
                {
                    Name = "MIT"
                }
            });

            // Add documentation for API v2.0 (future)
            c.SwaggerDoc("v2.0", new OpenApiInfo
            {
                Title = "PostComment API",
                Version = "v2.0",
                Description = "API for managing posts and comments with authentication (Enhanced Features)",
                Contact = new OpenApiContact
                {
                    Name = "Udemy API Support"
                },
                License = new OpenApiLicense
                {
                    Name = "MIT"
                }
            });

            // Add JWT Bearer security definition
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Description = "Please enter JWT with Bearer into field (Format: Bearer {token})",
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = JwtBearerDefaults.AuthenticationScheme,
                BearerFormat = "JWT"
            });

            // Add security requirement to all endpoints
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = JwtBearerDefaults.AuthenticationScheme
                        }
                    },
                    Array.Empty<string>()
                }
            });

            // Include XML documentation comments
            var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = System.IO.Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (System.IO.File.Exists(xmlPath))
            {
                c.IncludeXmlComments(xmlPath);
            }
        });

        return services;
    }
}

