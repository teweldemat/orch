using orch.common;
using orch.core.model;
using orch.wf.model;
using orch.wf.model.dto;

namespace orch.wf
{
    public interface IWfDatabase : IDisposable
    {
        IList<OTask> GetUserTasks(Guid userId, List<OTaskStatus>? filterStatuses = null);

        PagedList<OTask> GetUserTasksPaged(
            Guid userId,
            int index = 0,
            int count = 0,
            string? query = null,
            TaskSortFields sortBy = TaskSortFields.UpdateTime,
            SortOrder sortOrder = SortOrder.Desc,
            List<OTaskStatus>? filterStatuses = null,
            List<string>? taskTypeFilter = null);

        void CreateTask<T>(OCommand command, OTask task, T taskData, TaskNote note) where T : WfStateData;

        void AssignUsers(OCommand command, Guid taskId, IList<Guid> userId);

        void UnassignUsers(OCommand command, Guid taskId, IList<Guid> userId);

        void AssignFollowers(OCommand command, Guid taskId, IList<Guid> userId);

        void UnassignFollowers(OCommand command, Guid taskId, IList<Guid> userId);

        void ChangeState(OCommand command, Guid taskId, TaskChange taskState);

        IList<Guid> GetAssignedUsers(Guid taskId);

        IList<Guid> GetFollowingUsers(Guid taskId);

        OTask? GetTask(Guid id);

        object GetTaskData(Type type, Guid taskId);

        T GetTaskData<T>(Guid taskId) where T : WfStateData;

        TaskStatePair<T>? GetTaskStatePair<T>(Guid taskId) where T : WfStateData;

        void UpdateTaskData<T>(OCommand command, Guid taskId, T taskData, TaskNote note) where T : WfStateData;

        void UpdateTaskData<T>(OCommand command, Guid taskId, T taskData, TaskNote note, OTaskStatus? newState) where T : WfStateData;

        PagedList<OTask> GetTasks(List<Guid> taskTypes, int index, int count);

        T GetTaskDataByCommandId<T>(Guid commandId) where T : WfStateData;

        T GetTaskDataByRef<T>(String reference) where T : WfStateData;

        PagedList<T> GetOpenTasks<T>(Guid taskTypeId, int index, int count) where T : WfStateData;

        PagedList<T> GetClosedTasks<T>(Guid taskTypeId, int index, int count) where T : WfStateData;

        IList<OTask> GetAllTasks();

        PagedList<T> GetAllTasks<T>(
            Guid taskTypeId,
            int index,
            int count,
            string? query = null,
            TaskSortFields sortBy = TaskSortFields.CreateTime,
            SortOrder sortOrder = SortOrder.Desc,
            DateFilter? createDateFilter = null,
            DateFilter? updateDateFilter = null) where T : WfStateData;

        IList<TaskHistory> GetTaskHistory(Guid taskId);

        IList<TaskHistoryWithData> GetTaskHistoryWithData(Guid taskId);

        TaskHistory GetLastTaskChange(Guid taskId);

        void MonitorTask(OCommand command, Guid monitorTaskId, Guid monitoredTaskId);

        void RemoveMonitor(OCommand command, Guid monitorTaskId, Guid monitoredTaskId);

        IList<Guid> GetTaskMonitors(Guid taskId);

        void SendNotification(OCommand command, WfNotification notification, IList<Guid> users);

        void SetNotificateDelivered(OCommand command, Guid notificationId, Guid userId);

        WfNotification? GetNotification(Guid Id);

        PagedList<WfNotification> GetUserNotifications(Guid userId, long? fromTime, int index, int? count);

        WfNotificationTarget? GetNotificationTarget(Guid userId, Guid notificationId);

        IList<WfNotificationTarget> GetNotificationTargets(Guid Id);

        int GetUndeliveredNotificationsCount(Guid userId);

        List<TaskTypeCountPair> GetAssigneeOpenTaskCount(Guid assigneeId);
    }
}