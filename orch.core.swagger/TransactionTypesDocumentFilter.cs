using Microsoft.OpenApi.Models;
using orch.core.model.dto;
using orch.utils.web;

namespace orch.core.swagger
{
    public class TransactionTypesDocumentFilter : BaseDocumentFilter<CommandTypeInfo>
    {
        public static string DocName => "Commands";

        public TransactionTypesDocumentFilter() : base(DocName)
        {
        }


        public override IList<string> GetTags()
        {
            return OTransactionService.GetAllAssemblyNames();
        }

        public override IList<CommandTypeInfo> GetTypesByTag(string assemblyName)
        {
            return OTransactionService.GetTransactionTypesByAssembly(assemblyName);
        }

        public override void AddOperation(OpenApiDocument swaggerDoc, CommandTypeInfo commandTypeInfo, string assemblyName)
        {
            var path = $"/api/command?command_type={commandTypeInfo.Key}";
            var mapper = new SwaggerTypeMapper();

            var operation = new OpenApiOperation
            {
                OperationId = commandTypeInfo.Key,
                Summary = commandTypeInfo.TypeName,
                Description = GenerateDescription(commandTypeInfo),
                Parameters = new List<OpenApiParameter>
                {
                    new OpenApiParameter
                    {
                        Name = "query",
                        In = ParameterLocation.Query,
                        Required = false,
                        Schema = new OpenApiSchema { Type = "string" }
                    },
                    new OpenApiParameter
                    {
                        Name = "pars",
                        In = ParameterLocation.Query,
                        Required = false,
                        Schema = new OpenApiSchema { Type = "string" }
                    }
                },
                Responses = new OpenApiResponses
                {
                    ["201"] = new OpenApiResponse
                    {
                        Description = "Success",
                        Content = new Dictionary<string, OpenApiMediaType>
                        {
                            ["application/json"] = new OpenApiMediaType
                            {
                                Schema = mapper.MapTypeToSchema(typeof(string), swaggerDoc)
                            }
                        }
                    },
                    ["200"] = new OpenApiResponse
                    {
                        Description = "Success",
                        Content = new Dictionary<string, OpenApiMediaType>
                        {
                            ["application/json"] = new OpenApiMediaType
                            {
                                Schema = mapper.MapTypeToSchema(typeof(CommandQueryResult), swaggerDoc)
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
                RequestBody = new OpenApiRequestBody
                {
                    Required = true,
                    Content = new Dictionary<string, OpenApiMediaType>
                    {
                        ["application/json"] = new OpenApiMediaType
                        {
                            Schema = mapper.MapTypeToSchema(commandTypeInfo.Type, swaggerDoc)
                        }
                    }
                },

            };

            swaggerDoc.Paths.Add(path, new OpenApiPathItem
            {
                Operations = { { OperationType.Post, operation } }
            });

            operation.Tags = new List<OpenApiTag> { new OpenApiTag { Name = assemblyName } };
        }

        private static string GenerateDescription(CommandTypeInfo typeInfo)
        {
            List<string> segments = new()
            {
                $"TYPE ID: \"{typeInfo.TypeId}\"",
            };

            return string.Join(" • ", segments);
        }


    }
}
