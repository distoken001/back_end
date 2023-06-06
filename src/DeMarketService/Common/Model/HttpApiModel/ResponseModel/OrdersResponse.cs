using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace deMarketService.Common.Model.HttpApiModel.ResponseModel
{
    public class OrdersResponse
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
        /// 商品价格
        /// </summary>
        public decimal price { get; set; }
        /// <summary>
        /// 图片
        /// </summary>
        public string img { get; set; }
        /// <summary>
        /// 卖家质押
        /// </summary>
        public decimal seller_pledge { get; set; }
        /// <summary>
        /// 买家质押
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
        /// <summary>
        /// 链id
        /// </summary>
        public ChainEnum chain_id { get; set; }
        /// <summary>
        /// 买家比卖家额外多质押数量
        /// </summary>
        public decimal buyer_ex { get; set; }
        /// <summary>
        /// 合约地址
        /// </summary>
        public string contract { get; set; }
        /// <summary>
        /// 小时点
        /// </summary>
        public int decimals { get; set; }
        /// <summary>
        /// 链名称
        /// </summary>
        public string chain_name { get { if (chain_id == ChainEnum.Avalanche) { return "Avalanche C-Chain"; } else if (chain_id == ChainEnum.Polygon) { return "Polygon(Matic)"; }else return chain_id.ToString(); } }
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
                return price * amount / decimals_long;
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
        public  string seller_nick { get; set; }
    }
}
