using System;
using Grpc.Core;
using System.Numerics;
using Nethereum.ABI.FunctionEncoding.Attributes;
using CommonLibrary.Common.Common;

namespace ListenService.Model
{
    [FunctionOutput]
    public class OrderDTO
    {
        public string Seller { get; set; } // 卖家
        public string Buyer { get; set; } // 买家
        public string Name { get; set; } // 物品名称
        public BigInteger Price { get; set; } // 商品价格
        public BigInteger Amount { get; set; } // 物品数量
        public string Description { get; set; } // 描述
        public string Img { get; set; } // 商品图片
        public string Token { get; set; } // 质押代币合约地址
        public BigInteger SellerPledge { get; set; } // 卖家实际质押数量，如果是白名单用户则是手续费
        public BigInteger BuyerPledge { get; set; } // 买家实际质押数量（至少得是商品总价）
        public BigInteger BuyerEx { get; set; } // 买家超出商品总价质押部分（如果是白名单用户则是手续费）
        public OrderStatus Status { get; set; } // 订单状态
    }
}

