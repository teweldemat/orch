using Microsoft.EntityFrameworkCore.Metadata.Builders;
using orch.core.ef.Transaction.EntityConfigurations.common;
using orch.wf;

namespace orch.core.ef.workflow.EntityConfigurations.common
{
    public abstract class IWfStateDataTypeConfiguration<TEntity> : IChangePropsTypeConfiguration<TEntity> where TEntity : WfStateData
    {
        public override void Configure(EntityTypeBuilder<TEntity> builder)
        {
            base.Configure(builder);

            builder.HasKey(e => e.TaskId);

            builder.Property(e => e.TaskId)
                   .IsRequired();

            builder.Property(e => e.Reference)
                   .IsRequired();

            builder.Property(e => e.Description)
                   .IsRequired();
        }
    }
}
