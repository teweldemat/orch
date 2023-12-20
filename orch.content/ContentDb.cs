using Microsoft.EntityFrameworkCore;
using orch.content.model;

namespace orch.content
{
    public class ContentDb:DbContext
    {
        public const string CONTENT_SCHEMA = "content";
        public ContentDb(DbContextOptions options) : base(options)
        {
        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            orch.ef.core.TransactionDbContext.CamelizeNames(modelBuilder);
        }
    
    }
}
