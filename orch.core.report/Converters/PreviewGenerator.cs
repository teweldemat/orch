using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace orch.report.Generators
{
    public class ReportPreview
    {
        public string ViewName { get; }
        public object? Data { get; }

        public ReportPreview(string viewName, IDictionary<string, object>? data)
        {
            ViewName = viewName;
            Data = data;
        }

        public ReportPreview(string viewName, object? data)
        {
            ViewName = viewName;
            Data = data;
        }
    }

    public static class PreviewGenerator
    {
        public static ViewResult Generate(ReportPreview preview)
        {
            var viewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
            {
                Model = preview.Data
            };

            return new ViewResult
            {
                ViewName = preview.ViewName,
                ViewData = viewData
            };
        }
    }
}