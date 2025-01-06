using CommonLibrary.Common.Common;
using System;

namespace DeMarketAPI.Common.Model.HttpApiModel.ResponseModel
{
    public class InviterRebatesItemReponse
    {
        public long id { get; set; }

        /// <summary>
        /// 生成日期
        /// </summary>
        public DateTime date { get; set; }

        /// <summary>
        /// 订单id,非商品ID
        /// </summary>
        public long order_id { get; set; }

        /// <summary>
        /// 邀请人地址
        /// </summary>
        public string inviter_address { get; set; }

        /// <summary>
        /// 佣金
        /// </summary>
        public decimal amount { get; set; }

        /// <summary>
        /// 佣金
        /// </summary>
        public string amount_str
        { get { return amount.ToString(); } }

        /// <summary>
        /// 被邀请人
        /// </summary>
        public string be_inviter_address { get; set; }

        public DateTime create_time { get; set; }

        /// <summary>
        /// 交易的token名称
        /// </summary>
        public string token_name { get; set; }

        /// <summary>
        /// 是否已经返佣
        /// </summary>
        public int is_rebate { get; set; }

        /// <summary>
        /// token地址
        /// </summary>
        public string token_address { get; set; }

        /// <summary>
        /// 链ID
        /// </summary>
        public ChainEnum chain_id { get; set; }
    }
}