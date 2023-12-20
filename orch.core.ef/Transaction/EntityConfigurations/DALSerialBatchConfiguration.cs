using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using orch.core.ef.Transaction.EntityConfigurations.common;

namespace orch.core.ef.Transaction.Entities
{
    /// <summary>
    /// Configures the mapping between the DALSerialBatch entity and the corresponding database table.
    /// </summary>
    internal class DALSerialBatchConfiguration : IChangePropsTypeConfiguration<DALSerialBatch>
    {
        public override void Configure(EntityTypeBuilder<DALSerialBatch> builder)
        {
            base.Configure(builder);

            builder.ToTable(nameof(DALSerialBatch), OTransactionDbContext.CORE_SECHMA);
        }
    }
}