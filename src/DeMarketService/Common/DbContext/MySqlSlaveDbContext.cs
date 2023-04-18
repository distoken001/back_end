using deMarketService.Common.Model.DataEntityModel;
using Microsoft.EntityFrameworkCore;

namespace deMarketService.DbContext
{
    public class MySqlSlaveDbContext : Microsoft.EntityFrameworkCore.DbContext
    {
        public MySqlSlaveDbContext(DbContextOptions<MySqlSlaveDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }

        public virtual DbSet<event_logs> event_logs { get; set; }
        public virtual DbSet<orders> orders { get; set; }
        public virtual DbSet<users> users { get; set; }
    }
}
