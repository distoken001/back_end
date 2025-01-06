using CommonLibrary.Common.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CommonLibrary.Model.DataEntityModel
{
    [Table("card_not_opened")]
    public class card_not_opened
    {
        [Key]
        public int id { get; set; }

        public string card_type { get; set; }
        public string card_name { get; set; }
        public double price { get; set; }
        public int amount { get; set; }
        public DateTime create_time { get; set; }
        public DateTime? update_time { get; set; }
        public string updater { get; set; }
        public string creator { get; set; }
        public string buyer { get; set; }
        public ChainEnum chain_id { get; set; }
        public string contract { get; set; }
        public string token { get; set; }
        public string img { get; set; }
    }
}