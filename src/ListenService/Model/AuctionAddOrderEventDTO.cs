// 定义一个DTO类，用于解析事件数据
using System.Numerics;
using Nethereum.ABI.FunctionEncoding.Attributes;
namespace ListenService.Model
{
    [Event("AddOrder")]
    public class AuctionAddOrderEventDTO : IEventDTO
    {
        [Parameter("address", "defaulter", 1, true)]
        public string Defaulter { get; set; }
        [Parameter("uint256", "orderId", 2, true)]
        public BigInteger OrderId { get; set; }
        [Parameter("uint8", "status", 3, true)]
        public BigInteger Status { get; set; }
        [Parameter("address", "seller", 4, false)]
        public string Seller { get; set; }
        [Parameter("address", "buyer", 4, false)]
        public string Buyer { get; set; }

    }
}

