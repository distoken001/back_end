using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using CommonLibrary.Common.Common;

namespace DeMarketAPI.Common.Model.HttpApiModel.ResponseModel
{
    public class OrderResponse
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
        public double amount { get; set; }
        /// <summary>
        /// 商品价格
        /// </summary>
        public double price { get; set; }
        /// <summary>
        /// 图片
        /// </summary>
        public string img { get; set; }
        /// <summary>
        /// 原图片
        /// </summary>
        public string img_origin { get { if (!string.IsNullOrEmpty(img)) { return img.Replace("compress/", ""); } else return img; } }
        /// <summary>
        /// 卖家质押
        /// </summary>
        public double seller_pledge { get; set; }
        /// <summary>
        /// 买家质押
        /// </summary>
        public double buyer_pledge { get; set; }
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
        public double buyer_ex { get; set; }
        /// <summary>
        /// 合约地址
        /// </summary>
        public string contract { get; set; }
        /// <summary>
        /// 链名称
        /// </summary>
        public string chain_name { get { if (chain_id == ChainEnum.Bsc) { return "BNB Chain"; } else return chain_id.ToString(); } }

        /// <summary>
        /// 订单总价
        /// </summary>
        public double total_price
        {
            get
            {
                return price * amount;
            }
        }
        public string seller_ratio
        {
            get
            {
                double ratio = seller_pledge / total_price * 100; // 计算比例并转化为百分比
                string percentage = string.Format("{0:0.00}%", ratio); // 转化为字符串，并保留两位小数
                return percentage;
            }
        }
        public ChainTokenViewModel token_des { get; set; }
        public string seller_nick { get; set; }
        public string seller_email { get; set; }
        public string buyer_nick { get; set; }
        public string buyer_email { get; set; }
        /// <summary>
        /// 权重
        /// </summary>
        public int weight { get; set; }
        /// <summary>
        /// 收藏的人数量
        /// </summary>
        public int like_count { get; set; }
        /// <summary>
        /// 是否是自己喜欢
        /// </summary>
        public int is_like { get; set; }
        /// <summary>
        /// 卖家的nft
        /// </summary>
        public int[] seller_nfts { get; set; }
        public BelongUserEnum belong { get; set; }
    }
}
