using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using orch.core.model;

namespace orch.core.ef.Transaction.EntityConfigurations.common
{
    public abstract class IChangePropsTypeConfiguration<TEntity> : IEntityTypeConfiguration<TEntity> where TEntity : ChangeProps
    {
        public virtual void Configure(EntityTypeBuilder<TEntity> builder)
        {
            builder.Property(e => e.CreateCommandId)
                   .IsRequired();

            builder.Property(e => e.CreateTime)
                   .IsRequired();

            builder.Property(e => e.UpdateCommandId)
                   .IsRequired();

            builder.Property(e => e.UpdateTime)
                   .IsRequired();
        }
    }
}
