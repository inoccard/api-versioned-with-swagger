using System.Text;
using IdentityServer4.AccessTokenValidation;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi.Models;

namespace api_versioned_with_swagger.Swagger;

public static class SwaggerConfiguration
{
    public static void AddVersionedSwagger(this IServiceCollection services)
    {
        services.AddVersionedApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });

        services.AddApiVersioning();

        //add swagger json generation
        services.AddSwaggerGen(options =>
        {
            options.CustomSchemaIds(p => p.FullName);

            var provider = services.BuildServiceProvider().GetRequiredService<IApiVersionDescriptionProvider>();

            foreach (var description in provider.ApiVersionDescriptions)
            {
                var version = typeof(SwaggerConfiguration).Assembly.GetName().Version;

                var apiInfoDescription = new StringBuilder("api versioned");
                //describe the info
                var info = new OpenApiInfo
                {
                    Title = $"Api.Versioned {description.GroupName}",
                    Version = version?.ToString(),
                    Description = apiInfoDescription.ToString(),
                    License = new OpenApiLicense() { Name = $"App Version: {version?.Major}.{version?.Minor}.{version?.Build}" }
                };

                if (description.IsDeprecated)
                {
                    apiInfoDescription.Append(" This API version has been deprecated.");
                    info.Description = apiInfoDescription.ToString();
                }

                options.SwaggerDoc(description.GroupName, info);
                options.OperationFilter<DefaultHeaderFilter>();
            }

            //set default for non body parameters
            options.ParameterFilter<DefaultParametersFilter>();

            var securityDefinition = new OpenApiSecurityScheme
            {
                Description = "Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey
            };

            options.AddSecurityDefinition(IdentityServerAuthenticationDefaults.AuthenticationScheme, securityDefinition);

            var openApiSecurityScheme = new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            };

            options.AddSecurityRequirement(new OpenApiSecurityRequirement {
                {
                    openApiSecurityScheme,
                    Array.Empty<string>()
                }});
        });
    }

    public static void UseVersionedSwagger(this IApplicationBuilder app, IApiVersionDescriptionProvider apiVersionDescriptionProvider)
    {
        app.UseSwagger();

        //create the UI for swagger
        app.UseSwaggerUI(
            options =>
            {
                foreach (var description in apiVersionDescriptionProvider.ApiVersionDescriptions)
                {
                    options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json",
                        description.GroupName.ToUpperInvariant());
                    options.RoutePrefix = string.Empty;
                }
            });
    }
}