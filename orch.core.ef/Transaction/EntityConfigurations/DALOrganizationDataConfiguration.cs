using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using orch.core.ef.Transaction.EntityConfigurations.common;

namespace orch.core.ef.Transaction.Entities
{
    /// <summary>
    /// Configures the mapping between the DALOrganizationDataConfiguration entity and the corresponding database table.
    /// </summary>
    internal class DALOrganizationDataConfiguration : IChangePropsTypeConfiguration<DALOrganizationData>
    {
        public override void Configure(EntityTypeBuilder<DALOrganizationData> builder)
        {
            base.Configure(builder);

            builder.ToTable(nameof(DALOrganizationData), OTransactionDbContext.CORE_SECHMA);
        }
    }
}