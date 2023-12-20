using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using orch.core.ef.Transaction.EntityConfigurations.common;

namespace orch.core.ef.Transaction.Entities
{
    /// <summary>
    /// Configures the mapping between the DALCommandConfiguration entity and the corresponding database table.
    /// </summary>
    internal class DALContentReferenceConfiguration : IChangePropsTypeConfiguration<DALContentReference>
    {
        public override void Configure(EntityTypeBuilder<DALContentReference> builder)
        {
            base.Configure(builder);

            builder.ToTable(nameof(DALContentReference), OTransactionDbContext.CORE_SECHMA);
            builder.HasKey(DALContentReference => DALContentReference.Id);
        }
    }
}