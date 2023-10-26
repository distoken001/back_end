using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace deMarketService.Common.Model.DataEntityModel
{
    [Table("auction_user_like")]
    public class auction_user_like
    {
        [Key]
        public int id { get; set; }
        public string address { get; set; }
        public long order_id { get; set; }
        public int status { get; set; }
        public DateTime create_time { get; set; }
        public DateTime update_time { get; set; }
    }
}

