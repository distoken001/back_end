// 定义一个DTO类，用于解析事件数据
using System.Numerics;
using Nethereum.ABI.FunctionEncoding.Attributes;
namespace ListenService.Model
{
    [Event("PrizeClaimed")]
    public class PrizeClaimedEventDTO : IEventDTO
    {
        [Parameter("address", "user", 1, true)]
        public string User { get; set; }

        [Parameter("string", "cardType", 2, false)]
        public string BoxType { get; set; }

        [Parameter("uint256", "prize", 3, false)]
        public BigInteger Prize { get; set; }
    }
}

