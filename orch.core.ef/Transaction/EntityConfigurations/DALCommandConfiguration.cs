using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace orch.core.ef.Transaction.Entities
{
    /// <summary>
    /// Configures the mapping between the DALCommandConfiguration entity and the corresponding database table.
    /// </summary>
    internal class DALCommandConfiguration : IEntityTypeConfiguration<DALCommand>
    {
        public void Configure(EntityTypeBuilder<DALCommand> builder)
        {
            builder.ToTable(nameof(DALCommand), OTransactionDbContext.CORE_SECHMA);

            builder
                .HasOne(DALCommand => DALCommand.User)
                .WithMany()
                .HasForeignKey(DALCommand => DALCommand.UserId);

            builder
                .HasOne(DALCommand => DALCommand.Tran)
                .WithMany(DALOTransaction => DALOTransaction.Commands)
                .HasForeignKey(DALCommand => DALCommand.TranId);
        }
    }
}