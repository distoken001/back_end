using CommonLibrary.Common.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CommonLibrary.Model.DataEntityModel
{
    [Table("inviter_rebates")]
    public class inviter_rebates
    {
        [Key]
        public long id { get; set; }

        public DateTime date { get; set; }

        public long order_id { get; set; }

        public string inviter_address { get; set; }

        public decimal amount { get; set; }

        public string be_inviter_address { get; set; }

        public DateTime create_time { get; set; }

        public DateTime? update_time { get; set; }

        public string creater { get; set; }
        public string updater { get; set; }
        public string token_name { get; set; }
        public int is_rebate { get; set; }
        public string token_address { get; set; }
        public ChainEnum chain_id { get; set; }
    }
}