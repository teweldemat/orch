using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using orch.core.ef.Transaction.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace orch.core.ef.Transaction.EntityConfigurations
{

    internal class DALCommandTypeInfoConfiugration : IEntityTypeConfiguration<DALCommandTypeInfo>
    {
        public void Configure(EntityTypeBuilder<DALCommandTypeInfo> builder)
        {
            builder.ToTable(nameof(DALCommandTypeInfo), OTransactionDbContext.CORE_SECHMA);
            builder.HasKey(e => e.CommandTypeId);
        }
    }
}
