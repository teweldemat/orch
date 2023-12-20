using orch.core.model;

namespace orch.core
{
    public interface ICommandHandler
    {
        void SetData(OTransaction tran, OCommand command, object data, bool mainCommand);
        void Preprocess();
        void Execute();
        void PostExecute();
        string Summarize();
    }
}