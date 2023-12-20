using Microsoft.OpenApi.Models;
using orch.core.job;
using orch.utils.web;

namespace orch.core.swagger
{
    public class JobTypesDocumentFilter : BaseDocumentFilter<JobTypeInfo>
    {
        public static string DocName => "Jobs";

        public JobTypesDocumentFilter() : base(DocName)
        {
        }


        public override IList<string> GetTags()
        {
            return OJobService.GetAllAssemblyNames();
        }

        public override IList<JobTypeInfo> GetTypesByTag(string assemblyName)
        {
            return OJobService.GetTransactionTypesByAssembly(assemblyName);
        }

        public override void AddOperation(OpenApiDocument swaggerDoc, JobTypeInfo typeInfo, string assemblyName)
        {
            var path = $"/api/job?job_type={typeInfo.Key}";
            var mapper = new SwaggerTypeMapper();

            var operation = new OpenApiOperation
            {
                OperationId = typeInfo.Key,
                Summary = typeInfo.TypeName,
                Description = GenerateDescription(typeInfo),
                Responses = new OpenApiResponses
                {
                    ["200"] = new OpenApiResponse
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
                            Schema = mapper.MapTypeToSchema(typeInfo.Type, swaggerDoc)
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

        private static string GenerateDescription(JobTypeInfo typeInfo)
        {
            List<string> segments = new()
            {
                $"TYPE ID: \"{typeInfo.TypeId}\"",
                $"PROCESS TYPE: \"{typeInfo.ProcessType}\"",
            };

            return string.Join(" • ", segments);
        }


    }
}
