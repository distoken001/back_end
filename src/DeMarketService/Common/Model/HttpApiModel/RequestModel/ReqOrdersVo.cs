using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace deMarketService.Common.Model.HttpApiModel.RequestModel
{
    public class ReqOrdersVo
    {
        public int pageSize { get; set; }
        public int pageNum { get; set; } = 1;
        public int searchType { get; set; } // 0 查询卖单， 1查询买单
       

    }
}
