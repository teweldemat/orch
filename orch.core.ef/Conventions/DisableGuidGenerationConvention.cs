using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace orch.ef.Core.Conventions
{
    public class DisableGuidGenerationConvention : IModelFinalizingConvention
    {
        public void ProcessModelFinalizing(IConventionModelBuilder modelBuilder, IConventionContext<IConventionModelBuilder> context)
        {
            foreach (var entity in modelBuilder.Metadata.GetEntityTypes())
            {
                foreach (var property in entity.GetProperties())
                {
                    if (property.ClrType == typeof(Guid))
                    {
                        property.SetValueGenerated(ValueGenerated.Never);
                    }
                }
            }
        }
    }
}
