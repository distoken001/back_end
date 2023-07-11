using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace deMarketService.Common.Model.HttpApiModel.RequestModel
{
    public class OrderAuctionRequest
    {
        public int pageSize { get; set; } = 10;
        public int pageIndex { get; set; } = 1;
        /// <summary>
        /// 商品id
        /// </summary>
        public int? order_id { get; set; }
        /// <summary>
        /// 商品名称
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// 商品描述
        /// </summary>
        public string description { get; set; }
        /// <summary>
        /// 卖家地址
        /// </summary>
        public string seller { get; set; }
        /// <summary>
        /// 买家地址
        /// </summary>
        public string buyer { get; set; }

        ///// <summary>
        ///// 链id
        ///// </summary>
        public ChainEnum chain_id { get; set; }

    }
}
