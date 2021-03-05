using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;
using System.Linq;

namespace connectBase.Services.swagger
{
    public class BasicAuthOperationsFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var noAuthRequired = context.ApiDescription.CustomAttributes().Any(attr => attr.GetType() == typeof(AllowAnonymousAttribute));

            if (noAuthRequired) return;
            operation.Responses.Add("401", new OpenApiResponse { Description = "Unauthorized" });
            operation.Responses.Add("403", new OpenApiResponse { Description = "Forbidden" });

            operation.Security = new List<OpenApiSecurityRequirement>
                {
                    new OpenApiSecurityRequirement() {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = "API-Key"
                                },
                                Scheme = "oauth2",
                                Name = "API-Key",
                                In = ParameterLocation.Header,
                            },
                            new List<string>()
                        }
                    }
                };


            if (operation.Parameters == null) operation.Parameters = new List<OpenApiParameter>();
            var descriptor = context.ApiDescription.ActionDescriptor as ControllerActionDescriptor;
            if (descriptor != null && descriptor.ControllerName.StartsWith("Get") && descriptor.ActionName.StartsWith("GetFromIndexList"))
            {
                operation.Parameters.Add(new OpenApiParameter()
                {
                    Name = "indexList",
                    In = ParameterLocation.Query,
                    Description = "IndexList for querying specfic data in a table",
                    Required = true
                });
            }
            if (descriptor != null && descriptor.ControllerName.StartsWith("Get") && descriptor.ActionName.StartsWith("GetRange"))
            {
                operation.Parameters.Add(new OpenApiParameter()
                {
                    Name = "indexFields",
                    In = ParameterLocation.Query,
                    Description = "indexFields for querying a specfic range in a table",
                    Required = true
                });
            }
        }
    }
}