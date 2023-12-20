using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using orch.core.ef.Transaction.EntityConfigurations.common;
using orch.ef.workflow;
using orch.ef.workflow.entities;

namespace orch.core.ef.workflow.EntityConfigurations
{
    internal class DALOTaskConfiguration : IChangePropsTypeConfiguration<DALOTask>
    {
        public override void Configure(EntityTypeBuilder<DALOTask> builder)
        {
            base.Configure(builder);

            builder.ToTable(nameof(DALOTask), OWorkFlowDbContext.WORKFLOW_SCHEMA);
            builder.Property(x => x.TaskTypeId).IsRequired();
        }
    }

    internal class DALWfNotificationConfiguration : IEntityTypeConfiguration<DALWfNotification>
    {
        public void Configure(EntityTypeBuilder<DALWfNotification> builder)
        {
            builder.ToTable(nameof(DALWfNotification), OWorkFlowDbContext.WORKFLOW_SCHEMA);
            builder.HasOne(x => x.Task)
                .WithMany()
                .HasForeignKey(x => x.TaskId);
        }
    }

    internal class DALWfNotificationTargetConfiguration : IEntityTypeConfiguration<DALWfNotificationTarget>
    {
        public void Configure(EntityTypeBuilder<DALWfNotificationTarget> builder)
        {
            builder.ToTable(nameof(DALWfNotificationTarget), OWorkFlowDbContext.WORKFLOW_SCHEMA);
            builder.HasKey(x => new { x.NotificationId, x.UserId });
            builder.HasOne(x => x.Notification)
                .WithMany(x => x.Targets)
                .HasForeignKey(x => x.NotificationId);
        }
    }

    internal class DALTaskMonitorConfiguration : IEntityTypeConfiguration<DALTaskMonitor>
    {
        public void Configure(EntityTypeBuilder<DALTaskMonitor> builder)
        {
            builder.ToTable(nameof(DALTaskMonitor), OWorkFlowDbContext.WORKFLOW_SCHEMA);
            builder.HasOne(x => x.MonitorTask)
                .WithMany(x => x.MonitoredTasks)
                .HasForeignKey(x => x.MonitorTaskId);
            builder.HasOne(x => x.MonitoredTask)
                .WithMany(x => x.MonitoringTasks)
                .HasForeignKey(x => x.MonitoredTaskId);
        }
    }

    internal class DALTaskDataConfiguration : IEntityTypeConfiguration<DALTaskData>
    {
        public void Configure(EntityTypeBuilder<DALTaskData> builder)
        {
            builder.ToTable(nameof(DALTaskData), OWorkFlowDbContext.WORKFLOW_SCHEMA);
            builder.HasKey(e => e.TaskId);
            builder.Property(t => t.Data).HasColumnType("json");
            builder.HasOne(t => t.Task)
                .WithOne(t => t.Data);
        }
    }

    internal class DALCheckListItemConfiguration : IEntityTypeConfiguration<DALCheckListItem>
    {
        public void Configure(EntityTypeBuilder<DALCheckListItem> builder)
        {
            builder.ToTable(nameof(DALCheckListItem), OWorkFlowDbContext.WORKFLOW_SCHEMA);
            builder.HasKey(e => new { e.TaskId, e.Key });
            builder.HasOne(x => x.Task)
                .WithMany(x => x.CheckList);
        }
    }

    internal class DALTaskHistoryConfiguration : IEntityTypeConfiguration<DALTaskHistory>
    {
        public void Configure(EntityTypeBuilder<DALTaskHistory> builder)
        {
            builder.ToTable(nameof(DALTaskHistory), OWorkFlowDbContext.WORKFLOW_SCHEMA);
            builder.HasKey(e => new { e.TaskId, e.CommandId });
        }
    }

    internal class DALTaskNoteConfiguration : IEntityTypeConfiguration<DALTaskNote>
    {
        public void Configure(EntityTypeBuilder<DALTaskNote> builder)
        {
            builder.ToTable(nameof(DALTaskNote), OWorkFlowDbContext.WORKFLOW_SCHEMA);
            builder.HasOne(x => x.Task)
                .WithMany(x => x.Notes);
            builder.HasOne(x => x.History)
                .WithOne(x => x.Note)
                .HasForeignKey<DALTaskHistory>(x => x.NoteId);
        }
    }

    internal class DALTaskNoteContentItemConfiguration : IEntityTypeConfiguration<DALTaskNoteContentItem>
    {
        public void Configure(EntityTypeBuilder<DALTaskNoteContentItem> builder)
        {
            builder.ToTable(nameof(DALTaskNoteContentItem), OWorkFlowDbContext.WORKFLOW_SCHEMA);
            builder.HasKey(e => e.FileId);
            builder.HasOne(x => x.Note)
                .WithMany(x => x.Contents);
        }
    }

    internal class DALTaskAssigneeConfiguration : IEntityTypeConfiguration<DALTaskAssignee>
    {
        public void Configure(EntityTypeBuilder<DALTaskAssignee> builder)
        {
            builder.ToTable(nameof(DALTaskAssignee), OWorkFlowDbContext.WORKFLOW_SCHEMA);
            builder.HasKey(e => new { e.TaskId, e.UserId });
            builder.HasOne(x => x.Task)
                .WithMany(x => x.Assignees);
        }
    }

    internal class DALTaskFollowerConfiguration : IEntityTypeConfiguration<DALTaskFollower>
    {
        public void Configure(EntityTypeBuilder<DALTaskFollower> builder)
        {
            builder.ToTable(nameof(DALTaskFollower), OWorkFlowDbContext.WORKFLOW_SCHEMA);
            builder.HasKey(e => new { e.TaskId, e.UserId });
            builder.HasOne(x => x.Task)
                .WithMany(x => x.Followers);
        }
    }
}
