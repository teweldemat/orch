using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using orch.core.ef.Transaction.EntityConfigurations.common;

namespace orch.core.ef.Transaction.Entities
{
    /// <summary>
    /// Configures the mapping between the DALTransactionSystemInformation entity and the corresponding database table.
    /// </summary>
    internal class DALTransactionSystemInformationConfiguration : IChangePropsTypeConfiguration<DALTransactionSystemInformation>
    {
        public override void Configure(EntityTypeBuilder<DALTransactionSystemInformation> builder)
        {
            base.Configure(builder);

            builder.ToTable(nameof(DALTransactionSystemInformation), OTransactionDbContext.CORE_SECHMA);
            builder.HasKey(DALTransactionSystemInformation => DALTransactionSystemInformation.SystemId);
        }
    }
}