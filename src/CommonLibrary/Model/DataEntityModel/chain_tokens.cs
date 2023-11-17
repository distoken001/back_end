using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CommonLibrary.Common.Common;

namespace CommonLibrary.Model.DataEntityModel
{
    [Table("chain_tokens")]
    public class chain_tokens
    {
        [Key]
        public long id { get; set; }
        public ChainEnum chain_id { get; set; }

        public string token_name { get; set; }

        public string token_address { get; set;}

        public string icon  { get; set;}

        public DateTime create_time { get; set;}
        public int weight { get; set; }
        public int status { get; set; }
        public int decimals { get; set; }
    }
}
