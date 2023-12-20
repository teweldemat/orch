using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace orch.core.swagger
{
    public abstract class BaseDocumentFilter<OperationTypeInfo> : IDocumentFilter where OperationTypeInfo : class
    {
        private readonly string _docName;
        protected BaseDocumentFilter(string docName)
        {
            _docName = docName;
        }
        public abstract IList<string> GetTags();

        public abstract IList<OperationTypeInfo> GetTypesByTag(string tag);

        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            if (swaggerDoc.Info.Title != _docName)
            {
                return;
            }

            var pathsToRemove = swaggerDoc.Paths.Keys
                .Where(path => !path.StartsWith(_docName))
                .ToList();

            foreach (var path in pathsToRemove)
            {
                swaggerDoc.Paths.Remove(path);
            }

            var tags = GetTags();

            foreach (var tag in tags)
            {
                var typesByTag = GetTypesByTag(tag);
                foreach (var typeInfo in typesByTag)
                {
                    AddOperation(swaggerDoc, typeInfo, tag);
                }
            }
        }
        public abstract void AddOperation(OpenApiDocument swaggerDoc, OperationTypeInfo typeInfo, string tag);
    }
}
