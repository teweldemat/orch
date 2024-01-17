using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using orch.common;
using orch.core;
using orch.core.ef.System;
using orch.core.model;
using orch.ef.workflow.entities;
using orch.wf;
using orch.wf.model;
using orch.wf.model.dto;

namespace orch.ef.workflow
{
    [OView("wf")]
    public class EFWfDatabase : IWfDatabase
    {
        private readonly ITransactionDatabase _tranDb;
        private readonly OWorkFlowDbContext _dbContext;

        public EFWfDatabase(
            ITransactionDatabase tranDb,
            OWorkFlowDbContext dbContext)
        {
            ((EFTransactionDatabase)tranDb).AddTransactionDBContext(dbContext);

            _dbContext = dbContext;
            _tranDb = tranDb;
        }

        public void AssignFollowers(OCommand command, Guid taskId, IList<Guid> userId)
        {
            var task = GetTask(taskId);
            if (task == null)
                throw new InvalidOperationException($"Invalid task id {taskId}");
            foreach (var u in userId)
            {
                if (_tranDb.GetUserInfo(u) == null)
                    throw new InvalidOperationException($"Invalid user id {u}");
                if (_dbContext.TaskFollower.Where(x => x.UserId == u && x.TaskId == taskId).Any())
                    throw new InvalidOperationException($"User {u} is allready follower of task {taskId}");
                _dbContext.TaskFollower.Add(new DALTaskFollower
                {
                    TaskId = taskId,
                    UserId = u,
                    CommandId = command.Id
                });
            }
        }

        public void AssignUsers(OCommand command, Guid taskId, IList<Guid> userId)
        {
            var task = GetTask(taskId);
            if (task == null)
                throw new InvalidOperationException($"Invalid task id {taskId}");

            var taskAssignees = new List<DALTaskAssignee>();

            foreach (var u in userId)
            {
                if (_tranDb.GetUserInfo(u) == null)
                    throw new InvalidOperationException($"Invalid user id {u}");
                if (_dbContext.TaskAssignee.Where(x => x.UserId == u && x.TaskId == taskId).Any())
                    throw new InvalidOperationException($"User {u} is already assigned to task {taskId}");

                taskAssignees.Add(new DALTaskAssignee
                {
                    TaskId = taskId,
                    UserId = u,
                    CommandId = command.Id
                });
            }

            _dbContext.TaskAssignee.AddRange(taskAssignees);
            _dbContext.SaveChanges();
        }


        public void ChangeState(OCommand command, Guid taskId, TaskChange taskChange)
        {
            var task = GetTask(taskId);
            if (task == null)
                throw new InvalidOperationException($"Invalid task id {taskId}");
            var changed = false;

            var oldStatus = task.Status;

            /*if(taskChange.NewStatus!=null)
            {
                changed = true;
                task.Status = taskChange.NewStatus.Value;

            }*/
            if (taskChange.NewTitle != null || taskChange.NewDescription != null)
            {
                if (taskChange.NewTitle != null)
                    task.Name = taskChange.NewTitle;
                if (taskChange.NewDescription != null)
                    task.Description = taskChange.NewDescription;
                changed = true;
            }
            if (taskChange.RemoveCheckListItem != null)
                foreach (var checkListItem in taskChange.RemoveCheckListItem)
                {
                    _dbContext.TaskCheckLists.RemoveRange(_dbContext.TaskCheckLists.Where(x => x.TaskId == taskId
                        && x.Key == checkListItem));
                    changed = true;
                }
            if (taskChange.AddCheckListItem != null)
            {
                int seqNo;
                var all = _dbContext.TaskCheckLists.Where(x => x.TaskId == taskId);
                if (all.Any())
                    seqNo = all.Max(x => x.SeqNo) + 1;
                else
                    seqNo = 1;

                foreach (var checkListItem in taskChange.AddCheckListItem)
                {
                    _dbContext.TaskCheckLists.Add(new DALCheckListItem(checkListItem)
                    {
                        TaskId = taskId,
                        SeqNo = seqNo++,
                    });
                    changed = true;
                }
            }
            if (taskChange.ChangeChecklist != null)
                foreach (var checkListItem in taskChange.ChangeChecklist)
                {
                    var chk = _dbContext.TaskCheckLists.Where(x => x.TaskId == taskId && x.Key == checkListItem.Key).FirstOrDefault();
                    if (chk == null)
                        throw new InvalidOperationException($"Check list key {checkListItem.Key} is not in the checklist");
                    chk.State = checkListItem.State;
                    changed = true;
                }

            if (taskChange.Note != null)
            {
                foreach (var n in taskChange.Note)
                {
                    n.Id = Guid.NewGuid();
                    n.TaskId = taskId;
                    n.CommandId = command.Id;
                    _dbContext.TaskNotes.Add(new DALTaskNote(n));
                    int seqNo = 1;
                    if (n.Files != null)
                    {
                        foreach (var f in n.Files)
                        {
                            _tranDb.AddFileReference(command,
                                new ContentReference
                                {
                                    Id = Guid.NewGuid(),
                                    FileId = f,
                                    RefText = $"Attachment for task {task.Reference}"
                                }.SetCreate<ContentReference>(command));
                            var c = new DALTaskNoteContentItem
                            {
                                NoteId = n.Id,
                                FileId = f,
                                TranId = command.Id,
                                SeqNo = seqNo++,
                            };
                            _dbContext.NoteContents.Add(c);
                            changed = true;
                        }
                    }
                }
            }
            if (changed)
            {
                _dbContext.TaskHistory.Add(new DALTaskHistory
                {
                    TaskId = task.Id,
                    CommandId = command.Id,
                    CommandSeqNo = _tranDb.GetTransaction(command.TranId).SeqNo,
                    CommandTime = command.Time,
                    UserId = command.UserId!.Value,
                    MainCommand = command.MainCommand,
                    NoteId = null,
                    OldStatus = oldStatus,
                    NewStatus = task.Status,
                });
                var dalTask = new DALOTask(task).SetUpdate<DALOTask>(command);
                _dbContext.Tasks.Update(dalTask);
                _dbContext.SaveChanges();
                _dbContext.Entry(dalTask).State = EntityState.Detached;
            }
        }

        [OViewFunction]
        public OTask? GetTask(Guid id)
        {
            var ret = _dbContext.Tasks.AsNoTracking().Include(x => x.CheckList)
                .Where(x => x.Id == id)
                .Select(x => new OTask(x)
                {
                    CheckList = x.CheckList.OrderBy(x => x.SeqNo).AsEnumerable()
                        .Select(x => new CheckListItem(x))
                        .ToList()
                })
                .FirstOrDefault();
            return ret;
        }

        [OViewFunction]
        public IList<OTask> GetUserTasks(Guid userId, List<OTaskStatus>? filterStatuses = null)
        {
            IQueryable<DALOTask> query = _dbContext.TaskAssignee.Where(x => x.UserId == userId)
                .Join(_dbContext.Tasks.AsNoTracking(), x => x.TaskId, x => x.Id, (x, y) => y);

            if (filterStatuses != null && filterStatuses.Count > 0)
            {
                query = query.Where(x => filterStatuses.Contains(x.Status));
            }

            var ret = query.OrderByDescending(x => x.UpdateTime)
                .AsEnumerable()
                .Select(x => new OTask(x))
                .ToList();

            return ret;
        }

        public IList<Guid> GetUserTaskIds(
            Guid userId,
            List<OTaskStatus>? filterStatuses = null,
            List<Guid>? taskTypeIds = null)
        {
            var query = _dbContext.TaskAssignee
                .AsNoTracking()
                .Where(assignee => assignee.UserId == userId)
                .Select(assignee => assignee.Task);

            if (filterStatuses != null && filterStatuses.Count > 0)
            {
                query = query.Where(task => filterStatuses.Contains(task.Status));
            }

            if (taskTypeIds != null && taskTypeIds.Count > 0)
            {
                query = query.Where(task => taskTypeIds.Contains(task.TaskTypeId));
            }

            return query.Select(task => task.Id).ToList();
        }

        [OViewFunction(name: "GetUserTasksPaged")]
        public PagedList<OTask> GetUserTasksPaged(
            Guid userId,
            int index = 0,
            int count = 0,
            string? query = null,
            TaskSortFields sortBy = TaskSortFields.UpdateTime,
            SortOrder sortOrder = SortOrder.Desc,
            List<OTaskStatus>? filterStatuses = null,
            List<string>? taskTypeFilter = null)
        {

            var queryable = _dbContext.TaskAssignee
                .Include(x => x.Task)
                .Where(x => x.UserId == userId);

            if (filterStatuses is not null && filterStatuses.Count > 0)
            {
                queryable = queryable.Where(x => filterStatuses.Contains(x.Task.Status));
            }

            if (taskTypeFilter is not null)
            {
                var taskTypeIds = taskTypeFilter.Select(t =>
                {
                    var typeInfo = WfModule.GetWfTypeInfo(t)
                    ?? throw new InvalidOperationException($"WfTypeInfo not found for key: {t}");

                    return typeInfo.Id;
                }).ToList();

                queryable = queryable.Where(x => taskTypeIds.Contains(x.Task.TaskTypeId));
            }

            if (!string.IsNullOrEmpty(query))
            {
                queryable = queryable.Where(x =>
                EF.Functions.ILike(x.Task.Reference, $"%{query}%")
                || EF.Functions.ILike(x.Task.Name, $"%{query}%")
                || EF.Functions.ILike(x.Task.Description, $"%{query}%"));
            }

            switch (sortBy)
            {
                case TaskSortFields.Reference:
                    queryable = sortOrder == SortOrder.Asc
                        ? queryable.OrderBy(x => x.Task.Reference)
                        : queryable.OrderByDescending(x => x.Task.Reference);
                    break;
                case TaskSortFields.CreateTime:
                    queryable = sortOrder == SortOrder.Asc
                        ? queryable.OrderBy(x => x.Task.CreateTime)
                        : queryable.OrderByDescending(x => x.Task.CreateTime);
                    break;
                case TaskSortFields.UpdateTime:
                    queryable = sortOrder == SortOrder.Asc
                        ? queryable.OrderBy(x => x.Task.UpdateTime)
                        : queryable.OrderByDescending(x => x.Task.UpdateTime);
                    break;
            }



            return new PagedList<OTask>
            {
                Count = queryable.Count(),
                List = queryable.Skip(index).Take(count).AsEnumerable().Select(x => new OTask(x.Task)).ToList()
            };
        }

        [OViewFunction]
        public IList<OTask> GetAllTasks()//HACK
        {
            var ret =
                _dbContext.Tasks.AsNoTracking()
                .OrderByDescending(x => x.UpdateTime)
                .AsEnumerable()
                .Select(x => new OTask(x))
                .ToList();
            return ret;
        }

        public OTask? GetTask(string reference)
        {
            var ret = _dbContext.Tasks.AsNoTracking().Where(x => x.Reference == reference)
                .Select(x => new OTask(x))
                .FirstOrDefault();
            return ret;
        }

        private void assertCommand(OCommand command)
        {
            if (_tranDb.GetCommand(command.Id) == null)
                throw new orch.core.errors.InconsitentDbStateException($"Invalid command id{command.Id}");
        }

        public void CreateTask<T>(OCommand command, OTask task, T taskData, TaskNote note) where T : WfStateData
        {
            assertCommand(command);
            if (GetTask(task.Reference) != null)
                throw new InvalidOperationException($"Task refernce {task.Reference} is already used.");

            task.CreateTime = command.Time;
            task.SetCreate<ChangeProps>(command);
            task.Status = OTaskStatus.Started;
            var dalTask = new DALOTask(task);
            _dbContext.Tasks.Add(dalTask);
            if (taskData != null)
            {
                _dbContext.TaskData.Add(new DALTaskData
                {
                    TaskId = task.Id,
                    Data = Newtonsoft.Json.JsonConvert.SerializeObject(taskData)
                });
            }
            if (task.CheckList != null)
            {
                var o = 1;
                foreach (var chk in task.CheckList)
                {
                    _dbContext.TaskCheckLists.Add(new DALCheckListItem
                    {
                        Key = chk.Key,
                        Name = chk.Name,
                        SeqNo = o++,
                        State = chk.State,
                        TaskId = task.Id
                    });
                }
            }
            var noteId = InsertNote(command, task.Id, note);
            _dbContext.SaveChanges();
            _dbContext.TaskHistory.Add(new DALTaskHistory
            {
                TaskId = task.Id,
                CommandId = command.Id,
                CommandSeqNo = _tranDb.GetTransaction(command.TranId).SeqNo,
                CommandTime = command.Time,
                UserId = command.UserId.GetValueOrDefault(),
                MainCommand = command.MainCommand,
                NoteId = noteId,
                OldStatus = OTaskStatus.None,
                NewStatus = OTaskStatus.Started,
            });
            _dbContext.SaveChanges();
            _dbContext.Entry(dalTask).State = EntityState.Detached;
        }

        private Guid? InsertNote(OCommand command, Guid taskId, TaskNote note)
        {
            if (note != null)
            {
                note.Id = Guid.NewGuid();
                note.TaskId = taskId;
                note.CommandId = command.Id;
                _dbContext.TaskNotes.Add(new DALTaskNote(note));
                int seqNo = 1;
                if (note.Files != null)
                {
                    foreach (var f in note.Files)
                    {
                        _tranDb.AddFileReference(command,
                            new ContentReference
                            {
                                Id = Guid.NewGuid(),
                                FileId = f,
                                RefText = $"Task attachment"
                            }.SetCreate<ContentReference>(command));
                        var c = new DALTaskNoteContentItem
                        {
                            NoteId = note.Id,
                            FileId = f,
                            TranId = command.Id,
                            SeqNo = seqNo++,
                        };
                        _dbContext.NoteContents.Add(c);
                    }
                }
                return note.Id;
            }
            return null;
        }

        [OViewFunction]
        public IList<Guid> GetAssignedUsers(Guid taskId)
        {
            return _dbContext.TaskAssignee.Where(x => x.TaskId == taskId).Select(x => x.UserId).ToList();
        }

        public IList<Guid> GetFollowingUsers(Guid taskId)
        {
            return _dbContext.TaskFollower.Where(x => x.TaskId == taskId).Select(x => x.UserId).ToList();
        }

        public void UnassignFollowers(OCommand command, Guid taskId, IList<Guid> userId)
        {
            foreach (var u in userId)
                _dbContext.TaskFollower.RemoveRange(_dbContext.TaskFollower.Where(x => x.TaskId == taskId && x.UserId == u));
            _dbContext.SaveChanges();
        }

        public void UnassignUsers(OCommand command, Guid taskId, IList<Guid> userId)
        {
            foreach (var u in userId)
                _dbContext.TaskAssignee.RemoveRange(_dbContext.TaskAssignee.Where(x => x.TaskId == taskId && x.UserId == u));
            _dbContext.SaveChanges();
        }

        public object? GetTaskData(Type type, Guid taskId)
        {
            var x = _dbContext.TaskData.Where(x => x.TaskId == taskId).FirstOrDefault();
            if (x == null || x.Data == null)
                return null;
            return Newtonsoft.Json.JsonConvert.DeserializeObject(x.Data, type);
        }

        public T? GetTaskData<T>(Guid taskId) where T : WfStateData
        {
            var ret = GetTaskData(typeof(T), taskId);
            if (ret is T t)
                return t;
            return default;
        }

        public TaskStatePair<T>? GetTaskStatePair<T>(Guid taskId) where T : WfStateData
        {
            var task = _dbContext.Tasks
                                    .Include(t => t.Data)
                                    .Where(t => t.Id == taskId)
                                    .FirstOrDefault();

            if (task == null) return null;

            var taskInfo = WfModule.GetWfTypeInfo(task.TaskTypeId);

            return new TaskStatePair<T>()
            {
                Id = task.Id,
                TaskTypeKey = taskInfo.Key,
                TaskTypeName = taskInfo.Name,
                Task = new OTask(task),
                StateData = task.Data.Data == null ? null : Newtonsoft.Json.JsonConvert.DeserializeObject<T>(task.Data.Data)
            };
        }

        [OViewFunction]
        public PagedList<OTask> GetTasks(List<Guid> taskTypes, int index, int count)
        {
            var dblist = _dbContext.Tasks.AsNoTracking().Where(x => taskTypes.Contains(x.TaskTypeId));
            return new PagedList<OTask>
            {
                List = dblist
                .OrderByDescending(x => x.CreateTime)
                .Skip(index)
                .Take(count)
                .AsEnumerable()
                .Select(x => new OTask(x))
                .ToList(),
                Count = dblist.Count()
            };
        }

        public void UpdateTaskData<T>(OCommand command, Guid taskId, T taskData, TaskNote note) where T : WfStateData
        {
            UpdateTaskData(command, taskId, taskData, note, null);
        }
        public void UpdateTaskData<T>(OCommand command, Guid taskId, T taskData, TaskNote note, OTaskStatus? newState) where T : WfStateData
        {
            assertCommand(command);

            var task = _dbContext.TaskData.Where(x => x.TaskId == taskId).FirstOrDefault();
            if (task == null)
                throw new InvalidOperationException($"Task data {taskId} not found");
            if (taskData == null)
                task.Data = null;
            else
                task.Data = Newtonsoft.Json.JsonConvert.SerializeObject(taskData);
            var t = _dbContext.Tasks.AsNoTracking().FirstOrDefault(x => x.Id == taskId);
            var oldStatus = t.Status;
            if (newState != null)
            {
                t.Status = newState.Value;
                t = t.SetUpdate<DALOTask>(command);
                _dbContext.Tasks.Update(t);
            }

            var noteId = InsertNote(command, taskId, note);

            _dbContext.TaskHistory.Add(new DALTaskHistory
            {
                TaskId = taskId,
                CommandId = command.Id,
                CommandSeqNo = _tranDb.GetTransaction(command.TranId).SeqNo,
                CommandTime = command.Time,
                UserId = command.UserId.Value,
                MainCommand = command.MainCommand,
                NoteId = noteId,
                OldStatus = oldStatus,
                NewStatus = newState == null ? t.Status : newState.Value,
            });

            _dbContext.SaveChanges();
            _dbContext.Entry(t).State = EntityState.Detached;
        }
        [OViewFunction]
        public object? GetTaskDataByCommandId(Guid commandId)
        {
            var task = _dbContext.Tasks.AsNoTracking().Where(x => x.CreateCommandId == commandId).FirstOrDefault();
            if (task == null)
                return null;
            var taskInfo = WfModule.GetWfTypeInfo(task.TaskTypeId);
            return GetTaskData(taskInfo.Type, task.Id);
        }
        [OViewFunction]
        public object? GetTaskData(Guid taskId)
        {
            var task = _dbContext.Tasks.AsNoTracking().Where(x => x.Id == taskId).FirstOrDefault();
            if (task == null)
                return null;
            var taskInfo = WfModule.GetWfTypeInfo(task.TaskTypeId);
            return GetTaskData(taskInfo.Type, task.Id);
        }

        [OViewFunction]
        public object? GetTaskDataByRef(String reference)
        {
            var task = _dbContext.Tasks.AsNoTracking().Where(x => x.Reference == reference).FirstOrDefault();
            if (task == null)
                return null;
            var taskInfo = WfModule.GetWfTypeInfo(task.TaskTypeId);
            return GetTaskData(taskInfo.Type, task.Id);
        }
        public T GetTaskDataByCommandId<T>(Guid commandId) where T : WfStateData
        {
            var task = _dbContext.Tasks.AsNoTracking().Where(x => x.CreateCommandId == commandId).FirstOrDefault();
            if (task == null)
                return null;
            return GetTaskData<T>(task.Id);
        }

        public T? GetTaskDataByRef<T>(string reference) where T : WfStateData
        {
            var task = _dbContext.Tasks.AsNoTracking().Where(x => x.Reference == reference).FirstOrDefault();
            if (task == null)
                return null;
            return GetTaskData<T>(task.Id);
        }

        public PagedList<T> GetOpenTasks<T>(Guid taskTypeId, int index, int count) where T : WfStateData
        {
            var ret = new PagedList<T>();
            var list = _dbContext.Tasks.AsNoTracking().Where(
                x =>
                x.TaskTypeId == taskTypeId
                && x.Status != OTaskStatus.None
                && x.Status != OTaskStatus.Canceled
                && x.Status != OTaskStatus.Finished);
            ret.Count = list.Count();
            ret.List = list
                    .OrderByDescending(x => x.CreateTime)
                    .Skip(index).Take(count)
                    .ToList()
                    .Select(x => GetTaskData<T>(x.Id))
                    .ToList();
            return ret;
        }

        [OViewFunction]
        public PagedList<OTask> GetTasksByType(String taskTypeKey, int index, int length)
        {
            var taskType = WfModule.GetWfTypeInfo(taskTypeKey);
            OrchAssert.NoneNullDbObject(taskType, taskType);


            var q = _dbContext.Tasks.AsNoTracking().Where(x => x.TaskTypeId == taskType!.Id);

            var ret =
                q.OrderByDescending(x => x.UpdateTime)
                .Skip(index)
                .Take(length)
                .AsEnumerable()
                .Select(x => new OTask(x))
                .ToList();

            return new PagedList<OTask>
            {
                List = ret,
                Count = q.Count()
            };
        }

        public PagedList<T> GetAllTasks<T>(
            Guid taskTypeId,
            int index,
            int count,
            string? query = null,
            TaskSortFields sortBy = TaskSortFields.CreateTime,
            SortOrder sortOrder = SortOrder.Desc,
            DateFilter? createDateFilter = null,
            DateFilter? updateDateFilter = null)
            where T : WfStateData
        {
            var queryable = _dbContext.Tasks.AsNoTracking().Where(x => x.TaskTypeId == taskTypeId);

            if (createDateFilter != null)
            {
                if (createDateFilter.From.HasValue)
                    queryable = queryable.Where(x => x.CreateTime >= createDateFilter.From.Value);


                if (createDateFilter.To.HasValue)
                    queryable = queryable.Where(x => x.CreateTime <= createDateFilter.To.Value);
            }

            if (updateDateFilter != null)
            {
                if (updateDateFilter.From.HasValue)
                    queryable = queryable.Where(x => x.UpdateTime >= updateDateFilter.From.Value);


                if (updateDateFilter.To.HasValue)
                    queryable = queryable.Where(x => x.UpdateTime <= updateDateFilter.To.Value);
            }

            if (!string.IsNullOrEmpty(query))
            {
                queryable = queryable.Where(x =>
                EF.Functions.ILike(x.Reference, $"%{query}%")
                || EF.Functions.ILike(x.Name, $"%{query}%")
                || EF.Functions.ILike(x.Description, $"%{query}%"));
            }

            switch (sortBy)
            {
                case TaskSortFields.Reference:
                    queryable = sortOrder == SortOrder.Asc
                        ? queryable.OrderBy(x => x.Reference)
                        : queryable.OrderByDescending(x => x.Reference);
                    break;
                case TaskSortFields.CreateTime:
                    queryable = sortOrder == SortOrder.Asc
                        ? queryable.OrderBy(x => x.CreateTime)
                        : queryable.OrderByDescending(x => x.CreateTime);
                    break;
                case TaskSortFields.UpdateTime:
                    queryable = sortOrder == SortOrder.Asc
                        ? queryable.OrderBy(x => x.UpdateTime)
                        : queryable.OrderByDescending(x => x.UpdateTime);
                    break;
            }

            var totalCount = queryable.Count();

            var list = queryable
                .Skip(index)
                .Take(count)
                .ToList()
                .Select(x => GetTaskData<T>(x.Id))
                .ToList();

            return new PagedList<T>
            {
                Count = totalCount,
                List = list
            };
        }

        public PagedList<T> GetClosedTasks<T>(Guid taskTypeId, int index, int count) where T : WfStateData
        {
            var ret = new PagedList<T>();
            var list = _dbContext.Tasks.AsNoTracking().Where(
                x =>
                x.TaskTypeId == taskTypeId
                && (x.Status == OTaskStatus.None
                || x.Status == OTaskStatus.Canceled
                || x.Status == OTaskStatus.Finished));
            ret.Count = list.Count();
            ret.List = list
                    .OrderByDescending(x => x.CreateTime)
                    .Skip(index).Take(count)
                    .ToList()
                    .Select(x => GetTaskData<T>(x.Id))
                    .ToList();
            return ret;
        }

        public IList<TaskHistory> GetTaskHistory(Guid taskId)
        {
            var hist = _dbContext.TaskHistory.AsNoTracking()
                    .Where(x => x.TaskId == taskId)
                    .OrderByDescending(x => x.CommandTime)
                    .ThenByDescending(x => x.CommandSeqNo)
                    .AsEnumerable()
                    .Select(x => new TaskHistory(x))
                    .ToList();
            return hist;
        }

        public IList<TaskHistoryWithData> GetTaskHistoryWithData(Guid taskId)
        {
            var hist = _dbContext.TaskHistory.AsNoTracking()
                    .Where(x => x.TaskId == taskId)
                    .OrderByDescending(x => x.CommandSeqNo)
                    .ToList()
                    .Select(x =>
                    {
                        var data = _tranDb.GetCommand(x.CommandId);
                        var note = x.NoteId == null ? null : _dbContext.TaskNotes.Where(y => y.Id == x.NoteId).FirstOrDefault();
                        var attachments = x.NoteId == null ? null : _dbContext.NoteContents
                        .Where(y => y.NoteId == x.NoteId)
                        .OrderBy(y => y.SeqNo)
                        .Select(x => x.FileId)
                        .ToList();
                        var ret = new TaskHistoryWithData(x)
                        {
                            DataType = data == null ? "Unknown" : OTransactionService.GetTypeInfoById(data.DataTypeID).Key,
                            Note = note?.Note,
                            Attachments = attachments
                        };
                        return ret;
                    })
                    .ToList();
            return hist;
        }

        public TaskHistory GetLastTaskChange(Guid taskId)
        {
            return _dbContext.TaskHistory.AsNoTracking().Where(x => x.TaskId == taskId).OrderByDescending(x => x.CommandSeqNo).Take(1)
                    .Select(x => new TaskHistory(x))
                    .FirstOrDefault();
        }

        public void MonitorTask(OCommand command, Guid monitorTaskId, Guid monitoredTaskId)
        {
            _dbContext.TaskMonitors.Add(new DALTaskMonitor
            {
                MonitoredTaskId = monitoredTaskId,
                MonitorTaskId = monitorTaskId,
            });
            _dbContext.SaveChanges();
        }

        public void RemoveMonitor(OCommand command, Guid monitorTaskId, Guid monitoredTaskId)
        {
            var monitors = _dbContext.TaskMonitors.Where(x => x.MonitorTaskId == monitorTaskId
                && x.MonitoredTaskId == monitoredTaskId);
            _dbContext.TaskMonitors.RemoveRange(monitors);
            _dbContext.SaveChanges();
        }

        public IList<Guid> GetTaskMonitors(Guid taskId)
        {
            return _dbContext.TaskMonitors
                .Where(x => x.MonitoredTaskId == taskId).Select(x => x.MonitorTaskId)
                .ToList();
        }

        private static readonly JsonSerializerSettings settings = new JsonSerializerSettings
        {
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = null
            }
        };


        public void SendNotification(OCommand command, WfNotification notification, IList<Guid> users)
        {
            _dbContext.Notifications.Add(new DALWfNotification(notification));
            _dbContext.SaveChanges();

            var targets = users.Select(user => new DALWfNotificationTarget
            {
                NotificationId = notification.Id,
                UserId = user
            }).ToList();

            _dbContext.NotificationTargets.AddRange(targets);
            _dbContext.SaveChanges();
        }

        public void SetNotificateDelivered(OCommand command, Guid notificationId, Guid userId)
        {
            var target = _dbContext.NotificationTargets.FirstOrDefault(x => x.NotificationId == notificationId && x.UserId == userId);
            if (target == null)
                throw new InvalidOperationException($"Notification not found");
            target.DeliveredOn = command.Time;
            _dbContext.SaveChanges();
        }

        [OViewFunction]
        public WfNotification? GetNotification(Guid Id)
        {
            return _dbContext.Notifications
                .Where(n => n.Id == Id)
                .AsNoTracking()
                .Select(n => new WfNotification(n))
                .FirstOrDefault();
        }

        [OViewFunction]
        public PagedList<WfNotification> GetUserNotifications(Guid userId, long? fromTime, int index, int? count)
        {
            var query = _dbContext.Notifications
                .Where(n => n.Targets.Any(t => t.UserId == userId))
                .OrderByDescending(n => n.Time)
                .AsNoTracking()
                .AsQueryable();

            if (fromTime.HasValue)
            {
                query = query.Where(n => n.Time >= fromTime.Value);
            }

            var totalCount = query.Count();

            if (count.HasValue)
            {
                query = query.Skip(index).Take(count.Value);
            }

            var list = query.ToList();

            return new PagedList<WfNotification>
            {
                List = list.Select(x => new WfNotification(x)).ToList(),
                Count = totalCount
            };
        }

        [OViewFunction]
        public IList<WfNotificationTarget> GetNotificationTargets(Guid Id)
        {
            return _dbContext.NotificationTargets
                .Where(nt => nt.NotificationId == Id)
                .AsNoTracking()
                .Select(nt => new WfNotificationTarget(nt))
                .ToList();
        }

        [OViewFunction]
        public WfNotificationTarget? GetNotificationTarget(Guid userId, Guid notificationId)
        {
            return _dbContext.NotificationTargets
                .Where(nt => nt.UserId == userId && nt.NotificationId == notificationId)
                .AsNoTracking()
                .Select(nt => new WfNotificationTarget(nt))
                .FirstOrDefault();
        }

        [OViewFunction]
        public int GetUndeliveredNotificationsCount(Guid userId)
        {
            return _dbContext.NotificationTargets.Count(nt => nt.UserId == userId && nt.DeliveredOn == null);
        }

        public List<TaskTypeCountPair> GetAssigneeOpenTaskCount(Guid assigneeId)
        {
            var taskTypeCountList = _dbContext.TaskAssignee
                .Where(x => x.UserId == assigneeId)
                .Join(
                    _dbContext.Tasks.AsNoTracking(),
                    assignee => assignee.TaskId,
                    task => task.Id,
                    (assignee, task) => task
                )
                .Where(x =>
                    x.Status != OTaskStatus.None &&
                    x.Status != OTaskStatus.Canceled &&
                    x.Status != OTaskStatus.Finished
                )
                .GroupBy(x => x.TaskTypeId)
                .Select(group => new TaskTypeCountPair
                {
                    TaskTypeId = group.Key,
                    TaskTypeCount = group.Count()
                })
                .ToList();

            return taskTypeCountList;
        }

        [OViewFunction]
        public List<TaskTypeCountPair> GetUserOpenTaskCount([OViewUser] Guid userId)
        {
            return GetAssigneeOpenTaskCount(userId);
        }

        public void Dispose()
        {

        }
    }
}