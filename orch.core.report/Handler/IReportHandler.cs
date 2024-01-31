using Microsoft.AspNetCore.Mvc;

namespace orch.report.Handler
{
    /// <summary>
    /// Defines the contract for generating different types of reports.
    /// </summary>
    public interface IReportHandler
    {
        /// <summary>
        /// Sets the data used for generating the report.
        /// </summary>
        /// <param name="data">The data used for generating the report.</param>
        internal void SetData(object? data);
        public void PreProcess();

        /// <summary>
        /// Generates a preview of the report.
        /// </summary>
        /// <returns>A ViewResult object that represents the report's preview.</returns>
        public abstract ViewResult GeneratePreview();

        /// <summary>
        /// Generates a PDF file of the report.
        /// </summary>
        /// <param name="httpContext">The HttpContext object that represents the current HTTP request.</param>
        /// <returns>A FileContentResult object that represents the report's PDF file.</returns>
        public abstract Task<FileContentResult> GeneratePDF();

        /// <summary>
        /// Generates a CSV file of the report.
        /// </summary>
        /// <returns>A FileContentResult object that represents the report's CSV file.</returns>
        public abstract FileContentResult GenerateCSV();
    }
}