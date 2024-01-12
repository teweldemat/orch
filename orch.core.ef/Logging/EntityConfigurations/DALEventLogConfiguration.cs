using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using orch.core.ef.Logging.Entities;
using orch.core.ef.Transaction;

namespace orch.core.ef.Logging.EntityConfigurations
{
    internal class DALEventLogConfiguration : IEntityTypeConfiguration<DALEventLog>
    {
        public void Configure(EntityTypeBuilder<DALEventLog> builder)
        {
            builder.ToTable(nameof(DALEventLog), OTransactionDbContext.CORE_SECHMA);

            builder.HasKey(log => log.Id);

            builder.Property(log => log.Id).IsRequired();
            builder.Property(log => log.Message).IsRequired();
            builder.Property(log => log.Time).IsRequired();
            builder.Property(log => log.Level).HasConversion<string>().IsRequired();
            builder.Property(log => log.Reference).IsRequired(false);
            builder.Property(log => log.Data).HasColumnType("jsonb").IsRequired(false);

            builder.HasIndex(log => log.Reference);
            builder.HasIndex(log => log.CommandId);
            builder.HasIndex(log => log.TransactionId);
            builder.HasIndex(log => log.JobId);

        }
    }
}
