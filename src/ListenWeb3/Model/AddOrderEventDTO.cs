// 定义一个DTO类，用于解析事件数据
using System.Numerics;
using Nethereum.ABI.FunctionEncoding.Attributes;
namespace ListenWeb3.Model
{
    [Event("AddOrder")]
    public class AddOrderEventDTO : IEventDTO
    {
        [Parameter("address", "defaulter", 1, true)]
        public string Defaulter { get; set; }
        [Parameter("uint256", "orderId", 2, true)]
        public string OrderId { get; set; }
        [Parameter("uint8", "status", 3, true)]
        public BigInteger Status { get; set; }
        [Parameter("address", "seller", 4, false)]
        public BigInteger Seller { get; set; }
        [Parameter("address", "buyer", 4, false)]
        public BigInteger Buyer { get; set; }

    }
}

