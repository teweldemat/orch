using Microsoft.EntityFrameworkCore;
using Npgsql;
using orch.core.ef;
using System.Data.Common;
using orch.core.ef.Conventions;

namespace orch.ef.Core
{
    /// <summary>
    /// A base class for DbContext classes
    /// that automatically configures the connection and normalizes database artifacts.
    /// </summary>
    public class ODbContext : DbContext
    {
        public enum DatabaseMode
        {
            ReadOnly,
            ReadWrite
        }
        public DbConnection Connection { get; protected set; }

        private DatabaseMode _mode;
        public DatabaseMode Mode
        {
            get => _mode;
            set
            {
                if (value == _mode)
                {
                    var errorMessage = $"{GetType().BaseType?.Name} is already in {value} mode.\n" +
                                          "Cannot set the same mode again.";
                    throw new InvalidOperationException(errorMessage);
                }

                _mode = value;
            }
        }

        protected ODbContext(DbConnection connection)
        {
            Connection = connection;
            Mode = DatabaseMode.ReadWrite;
        }

        public override void Dispose()
        {

            Connection.Close();
            Connection.Dispose();
            base.Dispose();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            optionsBuilder.UseNpgsql(Connection, options =>
            {
                options.UseNetTopologySuite();
            });

        }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            base.ConfigureConventions(configurationBuilder);
            configurationBuilder.Conventions.Add(_ => new SnakeCaseNamingConvention());

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            RegisterDbFunction(modelBuilder, nameof(CoreEFmodule.Functions.ClusterDBSCAN));
            RegisterDbFunction(modelBuilder, nameof(CoreEFmodule.Functions.ClusterKMeans));
        }

        public override int SaveChanges()
        {
            CheckReadOnly();
            return base.SaveChanges();
        }
            
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            CheckReadOnly();
            return await base.SaveChangesAsync(cancellationToken);
        }

        protected static void RegisterDbFunction(ModelBuilder modelBuilder, string methodName)
        {
            var methodInfo = typeof(CoreEFDbFunctions).GetMethod(methodName);
            if (methodInfo != null)
            {
                modelBuilder.HasDbFunction(methodInfo);
            }
        }

        private void CheckReadOnly()
        {
            if (Mode == DatabaseMode.ReadOnly)
            {
                throw new InvalidOperationException(
                    $"{GetType().BaseType?.Name} is in read-only mode.\n" +
                    "Changes cannot be saved.");
            }
        }
    }


}