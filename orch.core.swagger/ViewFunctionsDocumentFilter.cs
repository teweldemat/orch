using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using orch.common;
using orch.utils.web;

namespace orch.core.swagger
{
    public class ViewFunctionsDocumentFilter : BaseDocumentFilter<ViewFunction>
    {
        public static string DocName => "View Functions";

        public ViewFunctionsDocumentFilter() : base(DocName)
        {
        }

        public override IList<string> GetTags()
        {
            return QueryComposer.GetViewFunctionCollectionNames();
        }

        public override IList<ViewFunction> GetTypesByTag(string tag)
        {
            return QueryComposer.GetViewFunctionCollection(tag).Select(x => x.Value).ToList();
        }

        public override void AddOperation(OpenApiDocument swaggerDoc, ViewFunction viewFunc, string tag)
        {
            var path = $"/api/vf?service={tag}&function={viewFunc.Name}";
            var mapper = new SwaggerTypeMapper();

            var operation = new OpenApiOperation
            {
                OperationId = viewFunc.Name,
                Description = viewFunc.Permissions != null && viewFunc.Permissions.Any()
                            ? $"Permissions: {string.Join(", ", viewFunc.Permissions)}"
                            : string.Empty,
                Parameters = viewFunc.Method.GetParameters().Where(p => !p.GetCustomAttributes(typeof(OViewUserAttribute), false).Any() && !p.ParameterType.IsByRef).Select(p =>
                {
                    var schema = mapper.MapTypeToSchema(p.ParameterType, swaggerDoc);
                    if (schema == null) return null;

                    var parameter = new OpenApiParameter
                    {
                        Name = p.Name,
                        In = ParameterLocation.Query,
                        Schema = schema,
                    };

                    if (!p.ParameterType.IsNullable())
                    {
                        parameter.Required = true;
                    }

                    if (p.HasDefaultValue)
                    {
                        parameter.Description = $"Default value: {p.DefaultValue}";
                    }

                    if (p.ParameterType.IsGenericType && (p.ParameterType.GetGenericTypeDefinition() == typeof(List<>) || p.ParameterType.GetGenericTypeDefinition() == typeof(IList<>)))
                    {
                        parameter.Explode = true;
                        parameter.Style = ParameterStyle.Simple;

                        var genericType = p.ParameterType.GetGenericArguments()[0];
                        parameter.Description = $"List of {genericType.Name}";
                    }

                    if (p.ParameterType == typeof(string) || (p.ParameterType == typeof(Guid?) || p.ParameterType == typeof(Guid)))
                    {
                        parameter.AllowEmptyValue = true;
                    }

                    if (p.ParameterType.IsEnum)
                    {
                        schema.Enum = Enum.GetNames(p.ParameterType).Select(n => new OpenApiString(n)).ToList<IOpenApiAny>();
                        schema.Type = "string";
                    }

                    return parameter;

                }).Where(p => p != null).ToList(),

                Responses = new OpenApiResponses
                {
                    ["200"] = new OpenApiResponse
                    {
                        Description = "Success",
                        Content = new Dictionary<string, OpenApiMediaType>
                        {
                            ["application/json"] = new OpenApiMediaType
                            {
                                Schema = mapper.MapTypeToSchema(viewFunc.Method.ReturnType, swaggerDoc)
                            }
                        }
                    },
                    ["500"] = new OpenApiResponse
                    {
                        Description = "Internal Server Error",
                        Content = new Dictionary<string, OpenApiMediaType>
                        {
                            ["application/json"] = new OpenApiMediaType
                            {
                                Schema = mapper.MapTypeToSchema(typeof(ErrorInfo), swaggerDoc)
                            }
                        }
                    }
                },

            };

            swaggerDoc.Paths.Add(path, new OpenApiPathItem
            {
                Operations = { { OperationType.Get, operation } }
            });

            operation.Tags = new List<OpenApiTag> { new OpenApiTag { Name = tag } };
        }
    }
}
