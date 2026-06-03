using orch.common;
using orch.utils.web;

namespace orch.wf.model
{
    public class TaskHistory : TaskHistoryProps
    {
        public TaskHistory() { }
        public TaskHistory(TaskHistoryProps props)
        => this.MapFromBase(props);
    }
    public class TaskHistoryWithData : TaskHistoryProps
    {
        public TaskHistoryWithData() { }
        public TaskHistoryWithData(TaskHistoryProps props)
        => this.MapFromBase(props);

        public string DataType { get; set; } = string.Empty;
        public string Note { get; set; } = string.Empty;
        public IList<Guid> Attachments { get; set; } = new List<Guid>();
    }

    public class TaskNote : TaskNoteProps
    {
        public TaskNote() { }
        public TaskNote(TaskNoteProps props)
        => this.MapFromBase(props);
        public IList<Guid> Files { get; set; } = new List<Guid>();
    }
}
    
