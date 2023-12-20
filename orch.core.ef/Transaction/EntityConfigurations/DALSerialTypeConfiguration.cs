using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using orch.core.ef.Transaction.EntityConfigurations.common;

namespace orch.core.ef.Transaction.Entities
{
    /// <summary>
    /// Configures the mapping between the DALSerialType entity and the corresponding database table.
    /// </summary>
    internal class DALSerialTypeConfiguration : IChangePropsTypeConfiguration<DALSerialType>
    {
        public override void Configure(EntityTypeBuilder<DALSerialType> builder)
        {
            base.Configure(builder);

            builder.ToTable(nameof(DALSerialType), OTransactionDbContext.CORE_SECHMA);
        }
    }
}