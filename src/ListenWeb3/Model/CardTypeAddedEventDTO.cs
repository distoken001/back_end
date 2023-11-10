// 定义一个DTO类，用于解析事件数据
using System.Numerics;
using Nethereum.ABI.FunctionEncoding.Attributes;
namespace ListenWeb3.Model
{

    public class CardTypeAddedEventDTO : IEventDTO
    {
        [Parameter("string", "cardType", 1, true)]
        public string CardType { get; set; }

        [Parameter("string", "cardName", 2, true)]
        public string CardName { get; set; }

        [Parameter("address", "tokenAddress", 3, true)]
        public string TokenAddress { get; set; }

        [Parameter("uint256", "price", 4, true)]
        public BigInteger Price { get; set; }

        [Parameter("uint256", "maxPrize", 5, true)]
        public BigInteger MaxPrize { get; set; }

        [Parameter("uint256", "maxPrizeProbability", 6, true)]
        public BigInteger MaxPrizeProbability { get; set; }

        [Parameter("uint256", "winningProbability", 7, true)]
        public BigInteger WinningProbability { get; set; }
    }
}
