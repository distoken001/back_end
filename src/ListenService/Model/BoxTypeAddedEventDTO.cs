// 定义一个DTO类，用于解析事件数据
using System.Numerics;
using Nethereum.ABI.FunctionEncoding.Attributes;
namespace ListenService.Model
{
    [Event("BoxTypeAdded")]
    public class BoxTypeAddedEventDTO : IEventDTO
    {
        [Parameter("string", "cardType", 1, false)]
        public string BoxType { get; set; }

        [Parameter("string", "cardName", 2, false)]
        public string BoxName { get; set; }

        [Parameter("address", "tokenAddress", 3, false)]
        public string TokenAddress { get; set; }

        [Parameter("uint256", "price", 4, false)]
        public BigInteger Price { get; set; }

        [Parameter("uint256", "maxPrize", 5, false)]
        public BigInteger MaxPrize { get; set; }

        [Parameter("uint256", "maxPrizeProbability", 6, false)]
        public BigInteger MaxPrizeProbability { get; set; }

        [Parameter("uint256", "winningProbability", 7, false)]
        public BigInteger WinningProbability { get; set; }
    }
}
