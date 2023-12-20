using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using orch.core.model;

namespace orch.core.ef
{
    public abstract class ChangePropsDbSet<TEntity> : DbSet<TEntity> where TEntity : ChangeProps
    {
        private readonly DbContext _context;

        protected ChangePropsDbSet(DbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public override EntityEntry<TEntity> Update(TEntity entity)
        {
            if (entity is not null)
            {
                // Get the entity type
                var entityType = _context.Model.FindEntityType(typeof(TEntity));

                if (entityType is null)
                {
                    throw new InvalidOperationException($"Entity type {typeof(TEntity)} not found in the DbContext model.");
                }

                // Find the property that represents the unique identifier (assuming it's the primary key)
                var primaryKeyProperty = entityType.FindPrimaryKey()?.Properties.FirstOrDefault();

                if (primaryKeyProperty != null)
                {
                    // Retrieve the existing entity from the database using the unique identifier
                    var primaryKeyValue = primaryKeyProperty.GetGetter().GetClrValue(entity);
                    var existingEntity = _context.Find<TEntity>(primaryKeyValue);

                    if (existingEntity != null)
                    {
                        // Capture the current values of CreateCommandId and CreateTime
                        var createCommandId = existingEntity.CreateCommandId;
                        var createTime = existingEntity.CreateTime;

                        // Apply the updates to the entity
                        _context.Entry(existingEntity).CurrentValues.SetValues(entity);

                        // Restore the captured values of CreateCommandId and CreateTime
                        existingEntity.CreateCommandId = createCommandId;
                        existingEntity.CreateTime = createTime;
                    }
                }
            }

            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            return base.Update(entity);
        }
    }
}
