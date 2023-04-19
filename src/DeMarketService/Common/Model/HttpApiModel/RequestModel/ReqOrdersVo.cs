using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static deMarketService.Model.EnumAll;

namespace deMarketService.Common.Model.HttpApiModel.RequestModel
{
    public class ReqOrdersVo
    {
        public int pageSize { get; set; }
        public int pageNum { get; set; } = 1;
        public int searchType { get; set; } // 0 查询卖单， 1查询买单

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
        public int amount { get; set; }
        /// <summary>
        /// 商品价格
        /// </summary>
        public int price { get; set; }
        /// <summary>
        /// 图片
        /// </summary>
        public string img { get; set; }
        /// <summary>
        /// 卖家质押
        /// </summary>
        public int seller_pledge { get; set; }
        /// <summary>
        /// 买家质押
        /// </summary>
        public int buyer_pledge { get; set; }
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
    }
}
