using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using orch.core.ef.Transaction;
using orch.core.ef.Transaction.Entities;

namespace orch.core.ef.Configuration
{
    internal class DALOJobConfiguration : IEntityTypeConfiguration<DALOJob>
    {
        public void Configure(EntityTypeBuilder<DALOJob> builder)
        {
            builder.ToTable(nameof(DALOJob), OTransactionDbContext.CORE_SECHMA);

            builder.HasKey(oJob => oJob.Id);

            builder.HasOne(dalOJob => dalOJob.User)
                   .WithMany()
                   .HasForeignKey(dalOJob => dalOJob.UserId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.Property(oJob => oJob.Time).IsRequired();

            builder.Property(oJob => oJob.UserId).IsRequired();

            builder.Property(oJob => oJob.TextSummary).IsRequired(false);

            builder.Property(oJob => oJob.DataTypeID).IsRequired();

            builder.Property(oJob => oJob.TextData).IsRequired(false).HasColumnType("jsonb");

            builder.Property(oJob => oJob.SystemID).IsRequired();
        }
    }
}
