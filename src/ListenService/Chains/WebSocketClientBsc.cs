using System;
using Nethereum.JsonRpc.Client;
using Nethereum.JsonRpc.WebSocketStreamingClient;
using Newtonsoft.Json;

namespace ListenService.Chains
{
	public class WebSocketClientBsc: StreamingWebSocketClient
    {
        // 定义构造函数以接受自定义参数
        public WebSocketClientBsc(string path,
                                              JsonSerializerSettings jsonSerializerSettings = null,
                                              ILogger log = null)
                                            
            : base(path, jsonSerializerSettings ?? DefaultJsonSerializerSettingsFactory.BuildDefaultJsonSerializerSettings(), log)
        {
            // 如果提供了自定义的连接超时，则覆盖默认值
          
        }
    }
}
