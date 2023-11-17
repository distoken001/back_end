// 定义一个DTO类，用于解析事件数据
using System.Numerics;
using Nethereum.ABI.FunctionEncoding.Attributes;
namespace ListenWeb3.Model

{
    [Event("CardTypeRemoved")]
    public class CardTypeRemovedEventDTO :IEventDTO
	{
        [Parameter("string", "cardType", 1, false)]
        public string CardType { get; set; }
    }
}

