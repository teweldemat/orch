using Microsoft.EntityFrameworkCore;
using orch.core.ef.Configuration;
using orch.core.ef.Logging.Entities;
using orch.core.ef.Logging.EntityConfigurations;
using orch.core.ef.Transaction.Entities;
using orch.core.ef.Transaction.EntityConfigurations;
using orch.ef.Core;
using System.Data.Common;

namespace orch.core.ef.Transaction
{
    /// <summary>
    /// Represents the database context used for managing transactions in the application.
    ///
    /// This base class is intended to be inherited by concrete classes in other projects,
    /// primarily for the purpose of creating database migrations in the consuming project rather
    /// than in the core project.
    /// </summary>
    public class OTransactionDbContext : ODbContext
    {
        public static readonly string CORE_SECHMA = "core";

        protected OTransactionDbContext(DbConnection connection) : base(connection)
        {
        }


        #region transaction
        protected internal DbSet<DALOTransaction> Transactions { get; set; }
        public DbSet<DALCommand> Commands { get; protected internal set; }
        protected internal DbSet<DALTransactionSystemInformation> TransactionSystemInformation { get; set; }
        #endregion

        #region job
        public DbSet<DALOJob> Jobs { get; protected internal set; }
        #endregion

        #region role & permission
        protected internal DbSet<DALPermission> Permissions { get; set; }
        protected internal DbSet<DALPermissionAdminInfo> PermssionAdmins { get; set; }
        protected internal DbSet<DALRole> Roles { get; set; }
        protected internal DbSet<DALRolePermission> PermissionRoles { get; set; }
        #endregion

        protected internal DbSet<DALContentReference> ContentReferences { get; set; }
        public DbSet<DALOrganizationData> Organizations { get; protected internal set; }

        #region serial
        protected internal DbSet<DALSerialBatch> SerialBatches { get; set; }
        protected internal DbSet<DALSerialNo> UsedSerials { get; set; }
        protected internal DbSet<DALSerialType> SerialTypes { get; set; }
        #endregion

        #region user
        public DbSet<DALUserInfo> Users { get; protected internal set; }
        protected internal DbSet<DALUserRole> UserRoles { get; set; }
        public DbSet<DALUserHistory> UserHistory { get; set; }
        #endregion

        #region logging
        public DbSet<DALEventLog> EventLogs { get; protected internal set; }
        #endregion

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            #region transaction
            modelBuilder.ApplyConfiguration(new DALCommandConfiguration());
            modelBuilder.ApplyConfiguration(new DALOTransactionConfiguration());
            modelBuilder.ApplyConfiguration(new DALTransactionSystemInformationConfiguration());
            #endregion

            #region job
            modelBuilder.ApplyConfiguration(new DALOJobConfiguration());
            #endregion

            modelBuilder.ApplyConfiguration(new DALContentReferenceConfiguration());
            modelBuilder.ApplyConfiguration(new DALOrganizationDataConfiguration());

            #region role & permission
            modelBuilder.ApplyConfiguration(new DALRoleConfiguration());
            modelBuilder.ApplyConfiguration(new DALRolePermissionConfiguration());
            modelBuilder.ApplyConfiguration(new DALPermissionAdminInfoConfiguration());
            modelBuilder.ApplyConfiguration(new DALPermissionConfiguration());
            #endregion

            #region serial
            modelBuilder.ApplyConfiguration(new DALSerialBatchConfiguration());
            modelBuilder.ApplyConfiguration(new DALSerialNoConfiguration());
            modelBuilder.ApplyConfiguration(new DALSerialTypeConfiguration());
            #endregion

            #region User
            modelBuilder.ApplyConfiguration(new DALUserHistoryConfiguration());
            modelBuilder.ApplyConfiguration(new DALUserInfoConfigurationConfiguration());
            modelBuilder.ApplyConfiguration(new DALUserRoleConfiguration());
            #endregion

            #region logging
            modelBuilder.ApplyConfiguration(new DALEventLogConfiguration());
            #endregion
        }
    }
}