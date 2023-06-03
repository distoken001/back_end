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
            builder.Entity<orders>().HasOne(i => i.chain_token).WithMany().HasForeignKey(i => i.token);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }

        public virtual DbSet<event_logs> event_logs { get; set; }
        public virtual DbSet<orders> orders { get; set; }
        public virtual DbSet<users> users { get; set; }
        public virtual DbSet<contacts> contacts { get; set; }
        public virtual DbSet<chain_tokens> chain_tokens { get; set; }
    }
}
