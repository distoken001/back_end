using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using CommonLibrary.Common.Common;
using Google.Protobuf.WellKnownTypes;

namespace deMarketService.Common.Model.HttpApiModel.ResponseModel
{
    public class OrderAuctionResponse
    {
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
        /// 商品价格（包含decimals的，一般不用）
        /// </summary>
        public decimal price { get; set; }
        /// <summary>
        /// 图片
        /// </summary>
        public string img { get; set; }
        /// <summary>
        /// 卖家质押（包含decimals的，一般不用）
        /// </summary>
        public decimal seller_pledge { get; set; }
        /// <summary>
        /// 买家质押（包含decimals的，一般不用）
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
        /// 订单状态（逻辑状态，不常用）
        /// </summary>
        public OrderAuctionStatus status { get; set; }
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
        /// 买家比卖家额外多质押数量（包含decimals的，一般不用）
        /// </summary>
        public decimal buyer_ex { get; set; }
        /// <summary>
        /// 合约地址
        /// </summary>
        public string contract { get; set; }
        /// <summary>
        /// 小输点个数
        /// </summary>
        public int decimals { get; set; }
        /// <summary>
        /// 链名称
        /// </summary>
        public string chain_name { get { if (chain_id == ChainEnum.Bsc) { return "BNB Chain"; } else return chain_id.ToString(); } }
        /// <summary>
        /// 算数
        /// </summary>
        public decimal decimals_long
        {
            get
            {
                return (decimal)Math.Pow(10, decimals);
            }
        }
        /// <summary>
        /// 买家比卖家额外多质押数量
        /// </summary>
        public decimal buyer_ex_actual { get { return buyer_ex / decimals_long; } }
        /// <summary>
        /// 卖家质押
        /// </summary>
        public decimal seller_pledge_actual { get { return seller_pledge / decimals_long; } }
        /// <summary>
        /// 买家质押
        /// </summary>
        public decimal buyer_pledge_actual { get { return buyer_pledge / decimals_long; } }
        /// <summary>
        /// 商品价格
        /// </summary>
        public decimal price_actual
        {
            get { return price / decimals_long; }
        }
        /// <summary>
        /// 订单总价
        /// </summary>
        public decimal total_price
        {
            get
            {
                return price_actual * amount;
            }
        }
        public string seller_ratio
        {
            get
            {
                decimal ratio = seller_pledge_actual / total_price * 100; // 计算比例并转化为百分比
                string percentage = string.Format("{0:0.00}%", ratio); // 转化为字符串，并保留两位小数
                return percentage;
            }
        }
        public ChainTokenViewModel token_des { get; set; }
        public string seller_nick { get; set; }
        public string seller_email { get; set; }
        /// <summary>
        /// 开始时间（时间戳）
        /// </summary>
        public long start_time { get; set; }
        /// <summary>
        /// 结束时间（时间戳）
        /// </summary>
        public long end_time { get; set; }
        public DateTime start_time_actual
        {
            get
            {
                DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(start_time);
                return dateTimeOffset.LocalDateTime;
            }
        }
        public DateTime end_time_actual
        {
            get
            {
                DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(end_time);
                return dateTimeOffset.LocalDateTime;
            }
        }
        /// <summary>
        /// 订单状态
        /// </summary>
        public OrderAuctionStatusActual status_actual
        {
            get { if (status == OrderAuctionStatus.SellerCancelWithoutDuty || status == OrderAuctionStatus.SellerBreak) { return OrderAuctionStatusActual.已结束; } else if (DateTime.Now < start_time_actual) { return OrderAuctionStatusActual.即将开始; } else if (DateTime.Now >= end_time_actual) { return OrderAuctionStatusActual.已结束; } else { return OrderAuctionStatusActual.进行中; } }
        }
        /// <summary>
        /// 次数
        /// </summary>
        public int count { get; set; }
        /// <summary>
        /// 卖方持有的nfts
        /// </summary>
        public int[] seller_nfts { get; set; }
        /// <summary>
        /// 收藏的人数量
        /// </summary>
        public int like_count { get; set; }
        /// <summary>
        /// 是否是自己喜欢
        /// </summary>
        public int is_like { get; set; }
        public BelongUserEnum belong { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string buyer_nick { get; set; }
    }
}