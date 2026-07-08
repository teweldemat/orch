namespace orch.utils.web
{
    public class ErrorInfo
    {
        public class ExceptionData
        {
            public string ExceptionType { get; set; } = string.Empty;
            public string Message { get; set; } = string.Empty;
            public string StackTrace { get; set; } = string.Empty;
        }

        public ErrorInfo()
        {
            Error = string.Empty;
            Exceptions = new List<ExceptionData>();
        }

        public string Error { get; set; }
        public IList<ExceptionData> Exceptions { get; set; }

        public ErrorInfo(string Error, Exception ex)
            : this(Error, ex, includeStackTrace: true)
        {
        }

        public ErrorInfo(string Error, Exception ex, bool includeStackTrace)
        {
            this.Error = Error;
            var list = new List<ExceptionData>();
            while (ex != null)
            {
                list.Insert(0,new ExceptionData()
                {
                    ExceptionType = ex.GetType().FullName,
                    Message = ex.Message,
                    StackTrace = includeStackTrace ? ex.StackTrace : string.Empty
                });
                ex = ex.InnerException;
            }
            Exceptions = list;
        }
    }
}
