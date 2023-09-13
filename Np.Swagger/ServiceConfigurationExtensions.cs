using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Np.Swagger.Filteres;

namespace Np.Swagger;

/// <summary>
/// расширение для конфигурации services
/// </summary>
public static class ServiceConfigurationExtensions
{
    /// <summary>
    /// Конфигурация swagger
    /// </summary>
    public static void ConfigureSwagger(
        this IServiceCollection services,
        string titleSwagger, 
        string version = "v1",
        Assembly[]? assemblies = null,
        bool useAuth = true)
    {
        if (assemblies is null)
            assemblies = AppDomain.CurrentDomain.GetAssemblies();
        else
            assemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(x => x.FullName != null && assemblies.Any() &&
                            assemblies.Any(a => a.FullName != null && x.FullName.StartsWith(a.FullName)))
                .ToArray();
        
        var xmlPaths = assemblies
            .Select(x => Path.Combine(AppContext.BaseDirectory, $"{x.GetName().Name}.xml"))
            .Where(File.Exists);
        
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc(version, new OpenApiInfo {Version = version, Title = titleSwagger});
            
            foreach (var path in xmlPaths)
            {
                options.IncludeXmlComments(path, true);
                options.SchemaFilter<EnumTypesSchemaFilter>(path);
            }
            
            if (!useAuth) return;
            
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = @"Введите JWT токен авторизации.",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                BearerFormat = "JWT",
                Scheme = "Bearer"
            });
            
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    new List<string>()
                }
            });
        });
    }
}
