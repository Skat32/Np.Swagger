using System.Xml.Linq;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Np.Swagger.Filteres;

/// <summary>
/// Добавление описания в перечисления для swagger
/// </summary>
public class EnumTypesSchemaFilter : ISchemaFilter
{
    private readonly XDocument _xmlComments;

    /// ctor
    public EnumTypesSchemaFilter(string xmlPath)
    {
        if (!File.Exists(xmlPath))
            throw new FileNotFoundException(); 
        
        _xmlComments = XDocument.Load(xmlPath);
    }
    
    /// <inheritdoc />
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (schema.Enum is not { Count: > 0 } 
            || context.Type is not { IsEnum: true }) 
            return;
        
        schema.Description += "<p>Содержит значения:</p><ul>";

        var fullTypeName = context.Type.FullName;

        foreach (var enumMemberName in Enum.GetValues(context.Type))
        {
            var enumMemberValue = Convert.ToInt64(enumMemberName);

            var fullEnumMemberName = $"F:{fullTypeName}.{enumMemberName}";

            var enumMemberComments = _xmlComments.Descendants("member")
                .FirstOrDefault(m => m.Attribute("name")!.Value.Equals
                    (fullEnumMemberName, StringComparison.OrdinalIgnoreCase));

            var summary = enumMemberComments?.Descendants("summary").FirstOrDefault();

            if (summary is null) 
                continue;

            schema.Description += $"<li><i>{enumMemberValue}</i> - { summary.Value.Trim()}</li>";
        }

        schema.Description += "</ul>";
    }
}
