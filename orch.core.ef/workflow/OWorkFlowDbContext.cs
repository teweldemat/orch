using Microsoft.EntityFrameworkCore;
using orch.core.ef.workflow.EntityConfigurations;
using orch.ef.Core;
using orch.ef.workflow.entities;
using System.Data.Common;

namespace orch.ef.workflow
{
    public class OWorkFlowDbContext : ODbContext
    {
        public const string WORKFLOW_SCHEMA = "wf";

        protected OWorkFlowDbContext(DbConnection connection) : base(connection)
        {
        }

        internal DbSet<DALOTask> Tasks { get; set; }
        internal DbSet<DALTaskData> TaskData { get; set; }
        internal DbSet<DALCheckListItem> TaskCheckLists { get; set; }
        internal DbSet<DALTaskHistory> TaskHistory { get; set; }
        internal DbSet<DALTaskNote> TaskNotes { get; set; }
        internal DbSet<DALTaskNoteContentItem> NoteContents { get; set; }
        internal DbSet<DALTaskAssignee> TaskAssignee { get; set; }
        internal DbSet<DALTaskFollower> TaskFollower { get; set; }
        internal DbSet<DALTaskMonitor> TaskMonitors { get; set; }
        internal DbSet<DALWfNotification> Notifications { get; set; }
        internal DbSet<DALWfNotificationTarget> NotificationTargets { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new DALOTaskConfiguration());
            modelBuilder.ApplyConfiguration(new DALWfNotificationConfiguration());
            modelBuilder.ApplyConfiguration(new DALWfNotificationTargetConfiguration());
            modelBuilder.ApplyConfiguration(new DALTaskMonitorConfiguration());
            modelBuilder.ApplyConfiguration(new DALTaskDataConfiguration());
            modelBuilder.ApplyConfiguration(new DALCheckListItemConfiguration());
            modelBuilder.ApplyConfiguration(new DALTaskHistoryConfiguration());
            modelBuilder.ApplyConfiguration(new DALTaskNoteConfiguration());
            modelBuilder.ApplyConfiguration(new DALTaskNoteContentItemConfiguration());
            modelBuilder.ApplyConfiguration(new DALTaskAssigneeConfiguration());
            modelBuilder.ApplyConfiguration(new DALTaskFollowerConfiguration());
        }
    }
}