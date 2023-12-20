namespace orch.core
{
    public interface IOHost
    {
        public long CurrentTime(TimeSpan? offset = null);
        public Guid NextGuid();
    }
}
