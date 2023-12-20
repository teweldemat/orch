using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using orch.core.ef.Transaction.EntityConfigurations.common;

namespace orch.core.ef.Transaction.Entities
{
    /// <summary>
    /// Configures the mapping between the DALSerialNo entity and the corresponding database table.
    /// </summary>
    internal class DALSerialNoConfiguration : IChangePropsTypeConfiguration<DALSerialNo>
    {
        public override void Configure(EntityTypeBuilder<DALSerialNo> builder)
        {
            base.Configure(builder);

            builder.ToTable(nameof(DALSerialNo), OTransactionDbContext.CORE_SECHMA);
            builder.HasKey(DALSerialNo => new { DALSerialNo.BatchId, DALSerialNo.Sn });
        }
    }
}