using orch.common;
using orch.wf.model;

namespace orch.ef.workflow.entities
{
    public class DALTaskNote : TaskNoteProps
    {
        public DALTaskNote()
        { }

        public DALTaskNote(TaskNoteProps props)
        => this.MapFromBase(props);

        public DALOTask Task { get; set; } = null!;
        public virtual ICollection<DALTaskNoteContentItem> Contents { get; set; } = new List<DALTaskNoteContentItem>();
        public DALTaskHistory History { get; set; } = null!;
    }

    public class DALTaskHistory : TaskHistoryProps
    {
        public DALTaskHistory()
        { }

        public DALTaskHistory(TaskHistoryProps props)
        => this.MapFromBase(props);

        public virtual DALOTask Task { get; set; } = null!;
        public virtual DALTaskNote? Note { get; set; }
    }

    public class DALTaskFollower : TaskFollowerProps
    {
        public DALTaskFollower()
        { }

        public DALTaskFollower(TaskFollowerProps props)
        => this.MapFromBase(props);

        public DALOTask Task { get; set; } = null!;
    }

    public class DALTaskAssignee : TaskAssigneeProps
    {
        public DALTaskAssignee()
        { }

        public DALTaskAssignee(TaskAssigneeProps props)
        => this.MapFromBase(props);

        public DALOTask Task { get; set; } = null!;
    }

    public class DALCheckListItem : CheckListItemProps
    {
        public DALCheckListItem()
        { }

        public DALCheckListItem(CheckListItemProps props)
        => this.MapFromBase(props);

        public Guid TaskId { get; set; }
        public DALOTask Task { get; set; } = null!;
        public int SeqNo { get; set; }
    }

    public class DALOTask : OTaskProps
    {
        public DALOTask()
        { }

        public DALOTask(OTaskProps props)
        => this.MapFromBase(props);

        public virtual ICollection<DALCheckListItem> CheckList { get; set; } = new List<DALCheckListItem>();
        public virtual DALTaskData Data { get; set; } = null!;
        public virtual ICollection<DALTaskNote> Notes { get; set; } = new List<DALTaskNote>();

        public virtual ICollection<DALTaskAssignee> Assignees { get; set; } = new List<DALTaskAssignee>();
        public virtual ICollection<DALTaskFollower> Followers { get; set; } = new List<DALTaskFollower>();
        public virtual ICollection<DALTaskMonitor> MonitoredTasks { get; set; } = new List<DALTaskMonitor>();
        public virtual ICollection<DALTaskMonitor> MonitoringTasks { get; set; } = new List<DALTaskMonitor>();
    }

    public class DALTaskMonitor
    {
        public Guid Id { get; set; }
        public Guid MonitorTaskId { get; set; }
        public Guid MonitoredTaskId { get; set; }
        public DALOTask MonitorTask { get; set; } = null!;

        public DALOTask MonitoredTask { get; set; } = null!;
    }

    internal class DALWfNotification : WfNotificationProps
    {
        public DALWfNotification()
        { }

        public DALWfNotification(WfNotificationProps props)
        => this.MapFromBase(props);

        public virtual DALOTask Task { get; set; } = null!;
        public virtual ICollection<DALWfNotificationTarget> Targets { get; set; } = new List<DALWfNotificationTarget>();
    }

    internal class DALWfNotificationTarget : WfNotificationTargetProps
    {
        public DALWfNotificationTarget()
        { }

        public DALWfNotificationTarget(WfNotificationTargetProps props)
        => this.MapFromBase(props);

        public virtual DALWfNotification Notification { get; set; } = null!;
    }
}