using orch.core.model;

namespace orch.core
{
    public abstract class CommandHandlerBase<T> : ICommandHandler
    {
        protected TransactionServiceCollection _services;

        #region data
        protected OCommand _commandInfo = null!;
        protected OTransaction _tranInfo = null!;
        protected T _commandData = default!;
        #endregion

        protected bool _mainCommand;

        protected CommandHandlerBase(TransactionServiceCollection services)
        {
            _services = services;
        }

        void ICommandHandler.Execute()
        {
            if (_mainCommand)
            {
                var root = _services.TranDb.GetRootUser();
                if (!(root == null || root.Id == _commandInfo.UserId))
                {
                    Authorize();
                }
            }
            Execute();
        }
        string ICommandHandler.Summarize()
        {
            if (_commandData is null)
                return string.Empty;

            var ret = Summarize(out var html);
            if (html)
            {
                return ret;
            }
            return $"<p>{System.Web.HttpUtility.HtmlEncode(ret)}</p>";
        }
        public void SetData(OTransaction tran, OCommand command, object data, bool mainCommand)
        {
            _commandInfo = command;
            _tranInfo = tran;
            _commandData = (T)data;
            _mainCommand = mainCommand;
        }
        public virtual void Preprocess()
        {
        }
        protected abstract void Execute();

        public virtual void PostExecute()
        {
        }

        public abstract string Summarize(out bool html);
        protected virtual void Authorize()
        {

        }

    }
}