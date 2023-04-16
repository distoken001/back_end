using deMarketService.Common.Model.DataEntityModel;
using Microsoft.EntityFrameworkCore;

namespace deMarketService.DbContext
{
    public class MySqlMasterDbContext : Microsoft.EntityFrameworkCore.DbContext
    {
        public MySqlMasterDbContext(DbContextOptions<MySqlMasterDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }
        public virtual DbSet<event_logs> event_logs { get; set; }
        public virtual DbSet<orders> orders { get; set; }
        public virtual DbSet<users> users { get; set; }
    }
}
