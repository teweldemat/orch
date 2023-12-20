namespace orch.utils.web
{
    public class ErrorInfo
    {
        public class ExceptionData
        {
            public string ExceptionType { get; set; }
            public string Message { get; set; }
            public string StackTrace { get; set; }
        }
        public string Error { get; set; }
        public IList<ExceptionData> Exceptions { get; set; }
        public ErrorInfo(string Error, Exception ex)
        {
            this.Error = Error;
            var list = new List<ExceptionData>();
            while (ex != null)
            {
                list.Insert(0,new ExceptionData() { ExceptionType = ex.GetType().FullName, Message = ex.Message, StackTrace = ex.StackTrace });
                ex = ex.InnerException;
            }
            Exceptions = list;
        }
    }
}
