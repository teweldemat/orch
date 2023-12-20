using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace orch.core.ef.Transaction.Entities
{
    /// <summary>
    /// Configures the mapping between the DALOTransactionConfiguration entity and the corresponding database table.
    /// </summary>
    internal class DALOTransactionConfiguration : IEntityTypeConfiguration<DALOTransaction>
    {
        public void Configure(EntityTypeBuilder<DALOTransaction> builder)
        {
            builder.ToTable(nameof(DALOTransaction), OTransactionDbContext.CORE_SECHMA);

            // Configures 'SeqNo' as an identity column, which auto-increments its value.
            builder.Property(builder => builder.SeqNo)
                   .UseIdentityByDefaultColumn();

            // Configure 'SeqNo' as a unique index.
            builder.HasIndex(DALOTransaction => DALOTransaction.SeqNo).IsUnique();

            builder.HasOne(DALOTransaction => DALOTransaction.User)
                .WithMany()
                .HasForeignKey(DALOTransaction => DALOTransaction.UserId);
        }
    }
}