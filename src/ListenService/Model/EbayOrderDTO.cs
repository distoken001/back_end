using CommonLibrary.Common.Common;
using Nethereum.ABI.FunctionEncoding.Attributes;
using System.Numerics;

namespace ListenService.Model
{
    [FunctionOutput]
    public class EbayOrderDTO
    {
        [Parameter("address", "seller", 1)]
        public string Seller { get; set; } // 卖家

        [Parameter("address", "buyer", 2)]
        public string Buyer { get; set; } // 买家

        [Parameter("string", "name", 3)]
        public string Name { get; set; } // 物品名称

        [Parameter("uint256", "price", 4)]
        public BigInteger Price { get; set; } // 商品价格

        [Parameter("uint256", "amount", 5)]
        public BigInteger Amount { get; set; } // 物品数量

        [Parameter("string", "description", 6)]
        public string Description { get; set; } // 描述

        [Parameter("string", "img", 7)]
        public string Img { get; set; } // 商品图片

        [Parameter("address", "token", 8)]
        public string Token { get; set; } // 质押代币合约地址

        [Parameter("uint256", "seller_pledge", 9)]
        public BigInteger SellerPledge { get; set; } // 卖家实际质押数量，如果是白名单用户则是手续费

        [Parameter("uint256", "buyer_pledge", 10)]
        public BigInteger BuyerPledge { get; set; } // 买家实际质押数量（至少得是商品总价）

        [Parameter("uint256", "buyer_ex", 11)]
        public BigInteger BuyerEx { get; set; } // 买家超出商品总价质押部分（如果是白名单用户则是手续费）

        [Parameter("uint8", "status", 12)]
        public OrderStatus Status { get; set; } // 订单状态
    }
}