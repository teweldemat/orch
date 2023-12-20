using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using orch.report;
using orch.report.Handler;
using orch.utils.web;

namespace orch.core.swagger
{
    public class ReportTypesPreviewDocumentFilter : BaseDocumentFilter<ReportTypeInfo>
    {
        public static string DocName => "Reports";

        public ReportTypesPreviewDocumentFilter() : base(DocName)
        {
        }

        public override IList<string> GetTags()
        {
            return OReportService.GetAllAssemblyNames();
        }

        public override IList<ReportTypeInfo> GetTypesByTag(string tag)
        {
            return OReportService.GetReportTypesByAssembly(tag);
        }

        public override void AddOperation(OpenApiDocument swaggerDoc, ReportTypeInfo reportTypeInfo, string assemblyName)
        {
            var path = $"/api/report/file?report_type={reportTypeInfo.Key}";
            var mapper = new SwaggerTypeMapper();

            var operation = new OpenApiOperation
            {
                OperationId = reportTypeInfo.Key,
                Summary = reportTypeInfo.TypeName,
                Description = reportTypeInfo.Permissions != null && reportTypeInfo.Permissions.Any()
                            ? $"Permissions: {string.Join(", ", reportTypeInfo.Permissions)}"
                            : string.Empty,
                Parameters = new List<OpenApiParameter>
                {
                    new OpenApiParameter
                    {
                        Name = "report_format",
                        In = ParameterLocation.Query,
                        Required = false,
                        Schema = new OpenApiSchema
                        {
                            Type = "string",
                            Enum = Enum.GetNames(typeof(ReportFormat)).Select(s => new OpenApiString(s) as IOpenApiAny).ToList(),
                            Default = new OpenApiString(Enum.GetName(typeof(ReportFormat), ReportFormat.PDF))
                        }
                    },
                    new OpenApiParameter
                    {
                        Name = "Accept-Language",
                        In = ParameterLocation.Header,
                        Required = false,
                        Description = "Optional language preference (e.g., en-US)",
                        Schema = new OpenApiSchema
                        {
                            Type = "string"
                        }
                    }
                },
                Responses = new OpenApiResponses
                {
                    ["200"] = new OpenApiResponse
                    {
                        Description = "Success",
                        Content = new Dictionary<string, OpenApiMediaType>
                        {
                            ["application/pdf"] = new OpenApiMediaType(),
                            ["text/csv"] = new OpenApiMediaType()
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
                    Content = new Dictionary<string, OpenApiMediaType>
                    {
                        ["application/json"] = new OpenApiMediaType
                        {
                            Schema = mapper.MapTypeToSchema(reportTypeInfo.Type, swaggerDoc)
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
    }
}
