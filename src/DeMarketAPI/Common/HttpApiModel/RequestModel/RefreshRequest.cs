using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommonLibrary.Common.Common;

namespace DeMarketAPI.Common.Model.HttpApiModel.RequestModel
{
    public class RefreshRequest
    {
        /// <summary>
        /// 链id
        /// </summary>
        public ChainEnum chain_id { get; set; }
    }
}
