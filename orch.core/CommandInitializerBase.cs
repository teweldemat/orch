using orch.core.model;

namespace orch.core
{
    public abstract class CommandInitializerBase<T> : ICommandInitializer
    {
        private IOHost _host;

        protected IOHost Host
        {
            get
            {
                return _host;
            }
        }

        protected OTransaction _tranInfo;
        protected OCommand _commandInfo;
        protected T _commandData;

        protected CommandInitializerBase(IOHost host)
        {
            _host = host;
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
