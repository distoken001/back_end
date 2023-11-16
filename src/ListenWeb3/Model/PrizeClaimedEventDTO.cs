// 定义一个DTO类，用于解析事件数据
using System.Numerics;
using Nethereum.ABI.FunctionEncoding.Attributes;
namespace ListenWeb3.Model
{
    [Event("PrizeClaimed")]
    public class PrizeClaimedEventDTO : IEventDTO
    {
        [Parameter("string", "user", 1, true)]
        public string User { get; set; }

        [Parameter("string", "cardType", 2, false)]
        public string CardType { get; set; }

        [Parameter("uint256", "prize", 4, false)]
        public BigInteger Prize { get; set; }
    }
}

