using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace LegalDocumentComparator.WebApi.Filters;

/// <summary>
/// Swagger operation filter to properly display file upload fields
/// </summary>
public class FileUploadOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var hasFileParameter = context.MethodInfo.GetParameters()
            .Any(p => p.ParameterType == typeof(IFormFile) || 
                     p.ParameterType == typeof(IEnumerable<IFormFile>));

        if (!hasFileParameter)
            return;

        operation.RequestBody = new OpenApiRequestBody
        {
            Content = new Dictionary<string, OpenApiMediaType>
            {
                ["multipart/form-data"] = new OpenApiMediaType
                {
                    Schema = new OpenApiSchema
                    {
                        Type = "object",
                        Properties = new Dictionary<string, OpenApiSchema>
                        {
                            ["file"] = new OpenApiSchema
                            {
                                Type = "string",
                                Format = "binary",
                                Description = "PDF file to upload"
                            },
                            ["name"] = new OpenApiSchema
                            {
                                Type = "string",
                                Description = "Document name"
                            },
                            ["versionNumber"] = new OpenApiSchema
                            {
                                Type = "string",
                                Description = "Version number (e.g., 1.0, 2.1)"
                            },
                            ["description"] = new OpenApiSchema
                            {
                                Type = "string",
                                Description = "Optional description",
                                Nullable = true
                            },
                            ["existingDocumentId"] = new OpenApiSchema
                            {
                                Type = "string",
                                Format = "uuid",
                                Description = "Optional: ID of existing document for versioning",
                                Nullable = true
                            }
                        },
                        Required = new HashSet<string> { "file", "name", "versionNumber" }
                    }
                }
            }
        };
    }
}
