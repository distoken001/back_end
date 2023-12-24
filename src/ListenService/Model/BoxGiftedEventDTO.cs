// 定义一个DTO类，用于解析事件数据
using System.Numerics;
using Nethereum.ABI.FunctionEncoding.Attributes;
namespace ListenService.Model
{
    [Event("BoxGifted")]
    public class BoxGiftedEventDTO : IEventDTO
    {
        [Parameter("address", "sender", 1, true)]
        public string Sender { get; set; }

        [Parameter("address", "recipient", 2, true)]
        public string Recipient { get; set; }

        [Parameter("string", "cardType", 3, false)]
        public string BoxType { get; set; }
        [Parameter("uint256", "numberOfBoxs", 4, false)]
        public BigInteger NumberOfBoxs { get; set; }
    }
}

