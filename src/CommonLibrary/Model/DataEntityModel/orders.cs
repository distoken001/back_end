using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace CommonLibrary.Common.Model.DataEntityModel
{
    [Table("orders")]
    public class orders
    {
        [Key]
        public long id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public long order_id { get; set; }
        /// <summary>
        /// 商品名字
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// 商品描述
        /// </summary>
        public string description { get; set; }
        /// <summary>
        /// 商品数量
        /// </summary>
        public decimal amount { get; set; }
        /// <summary>
        /// 商品价格
        /// </summary>
        public decimal price { get; set; }
        /// <summary>
        /// 图片
        /// </summary>
        public string img { get; set; }
        /// <summary>
        /// 卖家质押
        /// </summary>
        public decimal seller_pledge { get; set; }
        /// <summary>
        /// 买家质押
        /// </summary>
        public decimal buyer_pledge { get; set; }
        /// <summary>
        /// 卖家联系方式
        /// </summary>
        public string seller_contact { get; set; }
        /// <summary>
        /// 买家联系方式
        /// </summary>
        public string buyer_contact { get; set; }
        /// <summary>
        /// 订单状态
        /// </summary>
        public OrderStatus status { get; set; }


        public DateTime create_time { get; set; }

        public DateTime update_time { get; set; }

        public string updater { get; set; }

        public string creator { get; set; }
        /// <summary>
        /// 卖家地址
        /// </summary>
        public string seller { get; set; }
        /// <summary>
        /// 买家地址
        /// </summary>
        public string buyer { get; set; }
        /// <summary>
        /// token地址
        /// </summary>
        public string token { get; set; }
        /// <summary>
        /// 链id
        /// </summary>
        public ChainEnum chain_id { get; set; }
        /// <summary>
        /// 买家比卖家额外多质押数量
        /// </summary>
        public decimal buyer_ex { get; set; }
        /// <summary>
        /// 合约地址
        /// </summary>
        public string contract { get; set; }
        /// <summary>
        /// 小时点
        /// </summary>
        public int decimals { get; set; }
        /// <summary>
        /// 权重
        /// </summary>
        public int weight { get; set; }
    }
}
