using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommonLibrary.Common.Common;

namespace DeMarketAPI.Common.Model.HttpApiModel.RequestModel
{
    public class GetOrderListRequest
    {
        public int pageSize { get; set; } = 10;
        public int pageIndex { get; set; } = 1;
        /// <summary>
        /// 商品名称
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// 商品描述
        /// </summary>
        public string description { get; set; }
        ///// <summary>
        ///// 链id
        ///// </summary>
        public ChainEnum chain_id { get; set; }
        ///// <summary>
        /////1卖家发布 2买家发布 0全部
        ///// </summary>
        public PostWayEnum way { get; set; }
        

    }
}
