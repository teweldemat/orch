using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using orch.core.ef.System.Entities;

namespace orch.core.ef.System.EntityConfigurations
{
    /// <summary>
    /// Configures the mapping between the DALContentFileConfiguration entity and the corresponding database table.
    /// </summary>
    internal class DALContentFileConfiguration : IEntityTypeConfiguration<DALContentFile>
    {
        public void Configure(EntityTypeBuilder<DALContentFile> builder)
        {
            builder.ToTable(nameof(DALContentFile), OSystemDbContext.SYS_SCHEMA);
            builder.HasKey(DALContentFile => DALContentFile.FileId);
        }
    }
}