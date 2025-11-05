using orch.core.model;

namespace orch.core
{
    public interface ICommandFilter
    {
        (bool available, string message) CommandAvailable(OCommand commandInfo, object commandData);
    }
}
