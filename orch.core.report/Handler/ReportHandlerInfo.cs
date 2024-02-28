namespace orch.report.Handler
{
    /// <summary>
    /// An enumeration that represents the available report formats.
    /// </summary>
    public enum ReportFormat
    {
        CSV,
        PDF,
        XLS
    }

    /// <summary>
    /// A class that encapsulates information about a report handler.
    /// </summary>
    public class ReportHandlerInfo
    {
        /// <summary>
        /// Gets or sets the type of the report handler.
        /// </summary>
        public Type? HandlerType { get; set; }

        /// <summary>
        /// Gets or sets an array of types that represent the constructor parameters for the report handler.
        /// </summary>
        public Type[]? ConstructorParameters { get; set; }

        /// <summary>
        /// Gets or sets a list of report formats supported by the report handler.
        /// </summary>
        public List<ReportFormat>? SupportedFormats { get; set; }
    }
}