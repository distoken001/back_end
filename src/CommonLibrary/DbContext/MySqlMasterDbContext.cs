using CommonLibrary.Model.DataEntityModel;
using Microsoft.EntityFrameworkCore;

namespace CommonLibrary.DbContext
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

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }

        public virtual DbSet<event_logs> event_logs { get; set; }
        public virtual DbSet<orders> orders { get; set; }
        public virtual DbSet<post> post { get; set; }
        public virtual DbSet<orders_auction> orders_auction { get; set; }
        public virtual DbSet<users> users { get; set; }
        public virtual DbSet<user_nft> user_nft { get; set; }
        public virtual DbSet<cooperator> cooperator { get; set; }
        public virtual DbSet<contacts> contacts { get; set; }
        public virtual DbSet<chain_tokens> chain_tokens { get; set; }
        public virtual DbSet<inviter_rebates> inviter_rebates { get; set; }
        public virtual DbSet<ebay_user_like> ebay_user_like { get; set; }
        public virtual DbSet<auction_user_like> auction_user_like { get; set; }
        public virtual DbSet<telegram_user_chat> telegram_user_chat { get; set; }
        public virtual DbSet<card_type> card_type { get; set; }
        public virtual DbSet<card_not_opened> card_not_opened { get; set; }
        public virtual DbSet<card_opened> card_opened { get; set; }

    }
}
