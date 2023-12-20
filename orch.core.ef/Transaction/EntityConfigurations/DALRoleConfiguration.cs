using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using orch.core.ef.Transaction.EntityConfigurations.common;

namespace orch.core.ef.Transaction.Entities
{
    /// <summary>
    /// Configures the mapping between the DALRole entity and the corresponding database table.
    /// </summary>
    internal class DALRoleConfiguration : IChangePropsTypeConfiguration<DALRole>
    {
        public override void Configure(EntityTypeBuilder<DALRole> builder)
        {
            base.Configure(builder);

            builder.ToTable(nameof(DALRole), OTransactionDbContext.CORE_SECHMA);
        }
    }
}