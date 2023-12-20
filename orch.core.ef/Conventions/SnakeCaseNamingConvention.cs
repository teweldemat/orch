using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using orch.common;

namespace cis10.ef.Core.Conventions

{
    /// <summary>
    /// A custom code first convention that configures all database element names to snake_case
    /// </summary>
    internal class SnakeCaseNamingConvention : IModelFinalizingConvention
    {
        public void ProcessModelFinalizing(IConventionModelBuilder modelBuilder, IConventionContext<IConventionModelBuilder> context)
        {
            NormalizeDatabaseElementNames(modelBuilder.Metadata.GetEntityTypes());
        }

        /// <summary>
        /// Normalizes the names of database elements associated with the provided entity types.
        /// </summary>
        /// <param name="entityTypes">The entity types for which to normalize database element names.</param>
        private static void NormalizeDatabaseElementNames(IEnumerable<IConventionEntityType> entityTypes)
        {
            foreach (var entityType in entityTypes.Where(entityType => entityType?.GetTableName() != null))
            {
                // Normalize table name
                entityType.SetTableName(Helpers.ToSnakeCase(Helpers.StripDALPrefix(entityType.GetTableName()!)));

                // Normalize column names
                entityType!
                    .GetProperties()
                    .Where(property => property?.GetColumnName() != null)
                    .ToList()
                    .ForEach(property => property.SetColumnName(Helpers.ToSnakeCase(property!.GetColumnName())));

                // Normalize primary key names
                entityType!.GetKeys()
                    .Where(key => key?.GetName() != null)
                    .ToList()
                    .ForEach(key => key.SetName(Helpers.ToSnakeCase(Helpers.ToSnakeCase(key.GetName()!))));

                // Normalize foreign key constraint names
                entityType!.GetForeignKeys()
                    .Where(foreignKey => foreignKey?.GetConstraintName() != null)
                    .ToList()
                    .ForEach(foreignKey => foreignKey.SetConstraintName(Helpers.ToSnakeCase(foreignKey.GetConstraintName()!)));

                // Normalize index names
                entityType!.GetIndexes()
                    .Where(index => index?.Name != null)
                    .ToList()
                    .ForEach(index => index.SetDatabaseName(Helpers.ToSnakeCase(index.Name!)));
            }
        }



    }
}