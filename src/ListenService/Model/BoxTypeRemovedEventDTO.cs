// 定义一个DTO类，用于解析事件数据
using Nethereum.ABI.FunctionEncoding.Attributes;

namespace ListenService.Model

{
    [Event("BoxTypeRemoved")]
    public class BoxTypeRemovedEventDTO : IEventDTO
    {
        [Parameter("string", "cardType", 1, false)]
        public string BoxType { get; set; }
    }
}