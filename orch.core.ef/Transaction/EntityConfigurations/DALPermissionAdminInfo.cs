using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace orch.core.ef.Transaction.Entities
{
    /// <summary>
    /// Configures the mapping between the DALPermissionAdminInfoConfiguration entity and the corresponding database table.
    /// </summary>
    internal class DALPermissionAdminInfoConfiguration : IEntityTypeConfiguration<DALPermissionAdminInfo>
    {
        public void Configure(EntityTypeBuilder<DALPermissionAdminInfo> builder)
        {
            builder.ToTable(nameof(DALPermissionAdminInfo), OTransactionDbContext.CORE_SECHMA);
            builder.HasKey(DALPermissionAdminInfo => DALPermissionAdminInfo.PermissionAdminKey);
        }
    }
}