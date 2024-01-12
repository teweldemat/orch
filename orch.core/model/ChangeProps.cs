namespace orch.core.model
{
    public abstract class ChangeProps
    {
        public long CreateTime { get; set; }
        public Guid CreateCommandId { get; set; }
        public long UpdateTime { get; set; }
        public Guid UpdateCommandId { get; set; }
        public void CopyChangeProps(ChangeProps prop)
        {
            this.CreateTime = prop.CreateTime;
            this.CreateCommandId = prop.CreateCommandId;
            this.UpdateTime = prop.UpdateTime;
            this.UpdateCommandId = prop.UpdateCommandId;
        }
        public T SetCreate<T>(OCommand cmd) where T : ChangeProps
        {
            this.UpdateTime = this.CreateTime = cmd.Time;
            this.UpdateCommandId = this.CreateCommandId = cmd.Id;
            return (T)this;
        }
        public T SetUpdate<T>(OCommand cmd) where T : ChangeProps
        {
            this.UpdateTime = cmd.Time;
            this.UpdateCommandId = cmd.Id;
            return (T)this;
        }

        public void TransferCreate<T>(T props) where T : ChangeProps
        {
            this.CreateCommandId = props.CreateCommandId;
            this.CreateTime = props.CreateTime;
        }

    }
}
