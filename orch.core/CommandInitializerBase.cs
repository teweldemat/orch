using orch.core.model;

namespace orch.core
{
    public abstract class CommandInitializerBase<T> : ICommandInitializer
    {
        protected IOHost Host { get; }

        protected OTransaction _tranInfo = null!;
        protected OCommand _commandInfo = null!;
        protected T _commandData = default!;

        protected CommandInitializerBase(IOHost host)
        {
            Host = host;
        }

        public void SetData(OTransaction tranInfo, OCommand commandInfo, object data)
        {
            _tranInfo = tranInfo;
            _commandInfo = commandInfo;
            _commandData = (T)data;
        }

        public abstract void Init();


    }
}
