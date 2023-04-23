using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static deMarketService.Model.EnumAll;

namespace deMarketService.Common.Model.HttpApiModel.RequestModel
{
    public class ReqOrdersVo
    {
        public int pageSize { get; set; } = 10;
        public int pageIndex { get; set; } = 1;
        /// <summary>
        /// // 0 所有未购买交易(商品)， 1查询当前登录人所有交易
        /// </summary>
        public int searchType { get; set; }

        /// <summary>
        /// 商品名称
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// 商品id
        /// </summary>
        public int? order_id { get; set; }




        /// <summary>
        /// 卖家地址
        /// </summary>
        public string seller { get; set; }
        /// <summary>
        /// 买家地址
        /// </summary>
        public string buyer { get; set; }

        /// <summary>
        /// 链id
        /// </summary>
        public string chain_id { get; set; }

    }
}
