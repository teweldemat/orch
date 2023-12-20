namespace orch.wf.model
{
    public class TaskChange
    {
        //public orch.wf.model.OTaskStatus? NewStatus { get; set; }
        public OTask NewTask;
        public string NewTitle;
        public string NewDescription;
        public IList<TaskNote> Note;
        public IList<string> RemoveCheckListItem;
        public IList<CheckListItem> AddCheckListItem;
        public IList<CheckListItem> ChangeChecklist;
        public bool HasChange =>/*NewStatus!=null &&*/ (Note != null && Note.Count > 0) ||
            (RemoveCheckListItem != null && RemoveCheckListItem.Count > 0) ||
            (AddCheckListItem != null && AddCheckListItem.Count > 0) ||
            (ChangeChecklist != null && ChangeChecklist.Count > 0) ||
            NewTask != null || 
            NewDescription != null;
    }
}
