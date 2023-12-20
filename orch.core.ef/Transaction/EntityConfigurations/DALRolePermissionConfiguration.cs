using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using orch.core.ef.Transaction.Entities;

namespace orch.core.ef.Transaction.EntityConfigurations
{
    /// <summary>
    /// Configures the mapping between the DALRolePermission entity and the corresponding database table.
    /// </summary>
    internal class DALRolePermissionConfiguration : IEntityTypeConfiguration<DALRolePermission>
    {
        public void Configure(EntityTypeBuilder<DALRolePermission> builder)
        {
            builder.ToTable(nameof(DALRolePermission), OTransactionDbContext.CORE_SECHMA);
            builder.HasKey(DALRolePermission => new { DALRolePermission.RoleId, DALRolePermission.PermissionId });

            builder
                 .HasOne(DALRolePermission => DALRolePermission.Role)
                .WithMany(DALRole => DALRole.Permissions);

            builder
                .HasOne(DalRolePermission => DalRolePermission.Permission)
                .WithMany(DALPermission => DALPermission.Roles);
        }
    }
}