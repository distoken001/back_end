using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace deMarketService.Common.Model.HttpApiModel.RequestModel
{
    public class ReqLogsVo
    {
        public int pageSize { get; set; } = 10;
        public int pageIndex { get; set; } = 1;
        ///// <summary>
        ///// 链id
        ///// </summary>
        //public int chain_id { get; set; }

    }
}
