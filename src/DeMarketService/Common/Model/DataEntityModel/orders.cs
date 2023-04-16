using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace deMarketService.Common.Model.DataEntityModel
{
    [Table("orders")]
    public class orders
    {
        [Key]
        public int id { get; set; }

        public String goodsName { get; set; }

        public String goodsDesc { get; set; }

        public long quantity { get; set; }

        public long price { get; set; }

        public String goodsImg { get; set; }

        public String seller { get; set; }

        public String buyer { get; set; }

        public long sellerPledgeQuantity { get; set; }

        public long buyerPledgeQuantity { get; set; }

        public String sellerContact { get; set; }

        public String buyerContact { get; set; }

        public long status { get; set; }

        public DateTime createTime { get; set; }

        public DateTime updateTime { get; set; }

        public String updater { get; set; }

        public String creator { get; set; }
    }
}
