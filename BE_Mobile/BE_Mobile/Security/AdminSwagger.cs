using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace BE_Mobile.Security;

public sealed class AdminSwagger : IDocumentFilter
{
    public static void Configure(SwaggerGenOptions options)
    {
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            Description = "Use accessToken from POST /api/admin/auth/login."
        });
        options.DocumentFilter<AdminSwagger>();
    }

    public void Apply(OpenApiDocument document, DocumentFilterContext context)
    {
        foreach (var path in document.Paths.Where(p => p.Key.StartsWith("/api/admin/", StringComparison.Ordinal)
            && !p.Key.StartsWith("/api/admin/auth/", StringComparison.Ordinal)))
        {
            if (path.Value.Operations is null) continue;
            foreach (var operation in path.Value.Operations.Values)
                operation.Security = [new OpenApiSecurityRequirement { [new OpenApiSecuritySchemeReference("Bearer", document)] = [] }];
        }
    }
}
