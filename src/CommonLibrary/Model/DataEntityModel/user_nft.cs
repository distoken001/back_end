using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CommonLibrary.Common.Common;

namespace CommonLibrary.Model.DataEntityModel
{
    [Table("user_nft")]
    public class user_nft
    {
        [Key]
        public int id { get; set; }
        public String address { get; set; }
        public int nft { get; set; }
        public int status { get; set; }
        public DateTime create_time { get; set; }
        public DateTime update_time { get; set; }
        public string contract { get; set; }
        public ChainEnum chain_id { get; set; }

    }
}

