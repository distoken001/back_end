using System;
using CommonLibrary.Common.Common;

namespace DeMarketAPI.Common.Model.HttpApiModel.RequestModel
{
	public class SendEmailRequest
	{
		/// <summary>
		/// 订单id
		/// </summary>
		public long order_id { get; set; }
		/// <summary>
		/// 链id
		/// </summary>
        public ChainEnum chain_id { get; set; }
        /// <summary>
        /// 合约
        /// </summary>
        public string contract { get; set; }
    }
}

