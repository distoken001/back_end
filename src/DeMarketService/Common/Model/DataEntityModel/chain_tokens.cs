using System;
using System.ComponentModel.DataAnnotations;

namespace deMarketService.Common.Model.DataEntityModel
{
    public class chain_tokens
    {
        [Key]
        public long id { get; set; }
        public int chain_id { get; set; }

        public string token_name { get; set; }

        public string token_address { get; set;}

        public string icon  { get; set;}

        public DateTime create_time { get; set;}
    }
}
