using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using orch.core.ef.System.Entities;

namespace orch.core.ef.System.EntityConfigurations
{
    /// <summary>
    /// Configures the mapping between the DALAccessToken entity and the corresponding database table.
    /// </summary>
    internal class DALAccessTokenConfiguration : IEntityTypeConfiguration<DALAccessToken>
    {
        public void Configure(EntityTypeBuilder<DALAccessToken> builder)
        {
            builder.ToTable(nameof(DALAccessToken), OSystemDbContext.SYS_SCHEMA);
            builder.HasKey(DALAccessToken => DALAccessToken.Token);
        }
    }
}