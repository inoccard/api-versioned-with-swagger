using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace api_versioned_with_swagger.Swagger;

public class DefaultParametersFilter : IParameterFilter
{
    public void Apply(OpenApiParameter parameter, ParameterFilterContext context)
    {
        if (!(parameter is { } nonBodyParameter))
            return;

        nonBodyParameter.Description ??= context.ApiParameterDescription.ModelMetadata?.Description;

        if (context.ApiParameterDescription.RouteInfo != null)
            parameter.Required |= !context.ApiParameterDescription.RouteInfo.IsOptional;
    }
}
