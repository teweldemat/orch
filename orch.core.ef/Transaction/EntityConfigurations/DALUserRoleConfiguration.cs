using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using orch.core.ef.Transaction.Entities;

namespace orch.core.ef.Transaction.EntityConfigurations
{
    /// <summary>
    /// Configures the mapping between the DALUserRole entity and the corresponding database table.
    /// </summary>
    internal class DALUserRoleConfiguration : IEntityTypeConfiguration<DALUserRole>
    {
        public void Configure(EntityTypeBuilder<DALUserRole> builder)
        {
            builder.ToTable(nameof(DALUserRole), OTransactionDbContext.CORE_SECHMA);
            builder.HasKey(DALUserRole => new { DALUserRole.UserId, DALUserRole.RoleId });

            builder
                .HasOne(DALUserRole => DALUserRole.User)
                .WithMany(DALUserInfo => DALUserInfo.Roles)
                .HasForeignKey(DALUserRole => DALUserRole.UserId);

            builder.HasOne(DALUserRole => DALUserRole.Role)
                .WithMany(DALRole => DALRole.Users)
                .HasForeignKey(DALUserRole => DALUserRole.RoleId);
        }
    }
}