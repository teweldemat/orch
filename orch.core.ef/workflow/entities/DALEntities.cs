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

        public DALOTask Task { get; set; }
        public virtual ICollection<DALTaskNoteContentItem> Contents { get; set; }
        public DALTaskHistory History { get; set; }
    }

    public class DALTaskHistory : TaskHistoryProps
    {
        public DALTaskHistory()
        { }

        public DALTaskHistory(TaskHistoryProps props)
        => this.MapFromBase(props);

        public virtual DALOTask Task { get; set; }
        public virtual DALTaskNote? Note { get; set; }
    }

    public class DALTaskFollower : TaskFollowerProps
    {
        public DALTaskFollower()
        { }

        public DALTaskFollower(TaskFollowerProps props)
        => this.MapFromBase(props);

        public DALOTask Task { get; set; }
    }

    public class DALTaskAssignee : TaskAssigneeProps
    {
        public DALTaskAssignee()
        { }

        public DALTaskAssignee(TaskAssigneeProps props)
        => this.MapFromBase(props);

        public DALOTask Task { get; set; }
    }

    public class DALCheckListItem : CheckListItemProps
    {
        public DALCheckListItem()
        { }

        public DALCheckListItem(CheckListItemProps props)
        => this.MapFromBase(props);

        public Guid TaskId { get; set; }
        public DALOTask Task { get; set; }
        public int SeqNo { get; set; }
    }

    public class DALOTask : OTaskProps
    {
        public DALOTask()
        { }

        public DALOTask(OTaskProps props)
        => this.MapFromBase(props);

        public virtual ICollection<DALCheckListItem> CheckList { get; set; }
        public virtual DALTaskData Data { get; set; }
        public virtual ICollection<DALTaskNote> Notes { get; set; }

        public virtual ICollection<DALTaskAssignee> Assignees { get; set; }
        public virtual ICollection<DALTaskFollower> Followers { get; set; }
        public virtual ICollection<DALTaskMonitor> MonitoredTasks { get; set; }
        public virtual ICollection<DALTaskMonitor> MonitoringTasks { get; set; }
    }

    public class DALTaskMonitor
    {
        public Guid Id { get; set; }
        public Guid MonitorTaskId { get; set; }
        public Guid MonitoredTaskId { get; set; }
        public DALOTask MonitorTask { get; set; }

        public DALOTask MonitoredTask { get; set; }
    }

    internal class DALWfNotification : WfNotificationProps
    {
        public DALWfNotification()
        { }

        public DALWfNotification(WfNotificationProps props)
        => this.MapFromBase(props);

        public virtual DALOTask Task { get; set; }
        public virtual ICollection<DALWfNotificationTarget> Targets { get; set; }
    }

    internal class DALWfNotificationTarget : WfNotificationTargetProps
    {
        public DALWfNotificationTarget()
        { }

        public DALWfNotificationTarget(WfNotificationTargetProps props)
        => this.MapFromBase(props);

        public virtual DALWfNotification Notification { get; set; }
    }
}