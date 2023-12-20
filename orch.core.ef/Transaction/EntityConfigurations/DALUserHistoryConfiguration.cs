using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace orch.core.ef.Transaction.Entities
{
    /// <summary>
    /// Configures the mapping between the DALUserHistory entity and the corresponding database table.
    /// </summary>
    internal class DALUserHistoryConfiguration : IEntityTypeConfiguration<DALUserHistory>
    {
        public void Configure(EntityTypeBuilder<DALUserHistory> builder)
        {
            builder.ToTable(nameof(DALUserHistory), OTransactionDbContext.CORE_SECHMA);
            builder.HasKey(DALUserHistory => new { DALUserHistory.UserId, DALUserHistory.CommandId });
        }
    }
}