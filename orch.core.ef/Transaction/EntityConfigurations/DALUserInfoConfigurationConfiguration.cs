using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using orch.core.ef.Transaction.Entities;
using orch.core.ef.Transaction.EntityConfigurations.common;

namespace orch.core.ef.Transaction.EntityConfigurations
{
    /// <summary>
    /// Configures the mapping between the DALUserInfo entity and the corresponding database table.
    /// </summary>
    internal class DALUserInfoConfigurationConfiguration : IChangePropsTypeConfiguration<DALUserInfo>
    {
        public override void Configure(EntityTypeBuilder<DALUserInfo> builder)
        {
            base.Configure(builder);

            builder.ToTable(nameof(DALUserInfo), OTransactionDbContext.CORE_SECHMA)
                .HasKey(DALUserInfo => DALUserInfo.Id);
        }
    }
}