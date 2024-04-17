using orch.core.model;

namespace orch.core.replay
{
    public interface IReplayInterceptor<T>
    {
        void PreExecute(OCommand command, T data);
        void PostExecute(OCommand command, T data);
    }
}
