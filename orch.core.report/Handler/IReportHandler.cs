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
        internal protected void SetData(object? data);
        public void Preprocess();

        public abstract ViewResult GeneratePreview();
        public abstract Task<FileResult> GeneratePDF();
        public abstract FileResult GenerateCSV();
    }
}