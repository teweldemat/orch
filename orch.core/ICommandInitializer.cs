using orch.core.model;

namespace orch.core
{
    public interface ICommandInitializer
    {
        void SetData(OTransaction tranInfo, OCommand commandInfo, object data);
        void Init();
    }
}