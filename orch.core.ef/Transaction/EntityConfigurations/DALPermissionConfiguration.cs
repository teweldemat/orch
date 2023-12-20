using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using orch.core.ef.Transaction.EntityConfigurations.common;

namespace orch.core.ef.Transaction.Entities
{
    /// <summary>
    /// Configures the mapping between the DALPermission entity and the corresponding database table.
    /// </summary>
    internal class DALPermissionConfiguration : IChangePropsTypeConfiguration<DALPermission>
    {
        public override void Configure(EntityTypeBuilder<DALPermission> builder)
        {
            base.Configure(builder);

            builder.ToTable(nameof(DALPermission), OTransactionDbContext.CORE_SECHMA);
        }
    }
}