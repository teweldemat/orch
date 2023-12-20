using Microsoft.EntityFrameworkCore;
using orch.core.ef.System.Entities;
using orch.core.ef.System.EntityConfigurations;
using orch.ef.Core;
using System.Data.Common;

namespace orch.core.ef.System
{
    /// <summary>
    /// Represents the database context for the System database.
    /// Contains entities related to the overall system, such as access tokens and files.
    ///
    /// This base class is intended to be inherited by concrete classes in other projects,
    /// primarily for the purpose of creating database migrations in the consuming project rather
    /// than in the core project.
    /// </summary>
    public class OSystemDbContext : ODbContext
    {
        protected internal static readonly string SYS_SCHEMA = "sys";

        protected OSystemDbContext(DbConnection connection) : base(connection)
        {
        }

        protected internal DbSet<DALAccessToken> AccessTokens { get; set; }
        protected internal DbSet<DALContentFile> Files { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new DALAccessTokenConfiguration());
            modelBuilder.ApplyConfiguration(new DALContentFileConfiguration());

        }
    }
}