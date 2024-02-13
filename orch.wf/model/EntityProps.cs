using orch.core.model;

namespace orch.wf.model
{
    public class WfNotificationProps
    {
        public Guid Id { get; set; }
        public String Message { get; set; }
        public Guid? TaskId { get; set; }
        public long Time { get; set; }
    }
    public class WfNotificationTargetProps
    {
        public Guid NotificationId { get; set; }
        public Guid UserId { get; set; }
        public long? DeliveredOn { get; set; }
    }
    public class OTaskTypeProps
    {
        public Guid Id { get; set; }
        public string Key { get; set; }
        public string Name { get; set; }
    }
    public enum OTaskStatus
    {
        None,
        Started,
        Finished,
        Canceled,
        Suspended
    }
    public class OTaskProps : ChangeProps
    {
        public Guid Id { get; set; }
        public string Reference { get; set; }
        public Guid TaskTypeId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public OTaskStatus Status { get; set; }
    }
    public enum CheckListState
    {
        NotDone,
        Done,
        CrossedOut
    }
    public class CheckListStatusProps
    {
        public string Key { get; set; }
        public CheckListState State { get; set; }
    }
    public class CheckListItemProps
    {
        public string Key { get; set; }
        public string Name { get; set; }
        public CheckListState State { get; set; }

    }
    public class TaskAssigneeProps
    {
        public Guid TaskId { get; set; }
        public Guid UserId { get; set; }
        public Guid CommandId { get; set; }
    }
    public class TaskFollowerProps
    {
        public Guid TaskId { get; set; }
        public Guid UserId { get; set; }
        public Guid CommandId { get; set; }
    }
    public class TaskHistoryProps
    {
        public Guid TaskId { get; set; }
        public Guid? UserId { get; set; }
        public Guid CommandId { get; set; }
        public long CommandTime { get; set; }
        public long CommandSeqNo { get; set; }
        public bool MainCommand { get; set; }
        public Guid? NoteId { get; set; }
        public OTaskStatus OldStatus { get; set; } = OTaskStatus.None;
        public OTaskStatus NewStatus { get; set; } = OTaskStatus.None;

    }
    public class TaskNoteProps
    {
        public Guid Id { get; set; }
        public Guid TaskId { get; set; }
        public string Note { get; set; }
        public Guid CommandId { get; set; }
    }
}

