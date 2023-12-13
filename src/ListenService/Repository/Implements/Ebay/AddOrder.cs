using Nethereum.Contracts;
using CommonLibrary.DbContext;
using Nethereum.JsonRpc.WebSocketStreamingClient;
using Nethereum.RPC.Reactive.Eth.Subscriptions;
using Newtonsoft.Json;
using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Nethereum.ABI.Model;
using ListenService.Model;
using Nethereum.JsonRpc.Client;
using CommonLibrary.Model.DataEntityModel;
using CommonLibrary.Common.Common;
using ListenService.Repository.Interfaces;
using System.Net.WebSockets;
using Newtonsoft.Json.Linq;
using Nethereum.Web3;
using Nethereum.RPC;

namespace ListenService.Repository.Implements
{
    public class AddOrder : IAddOrder
    {
        private readonly IConfiguration _configuration;
        private readonly MySqlMasterDbContext _masterDbContext;
        public AddOrder(IConfiguration configuration, MySqlMasterDbContext mySqlMasterDbContext)
        {
            _configuration = configuration;
            _masterDbContext = mySqlMasterDbContext;
        }
        public async Task StartAsync(string nodeWss,string nodeHttps string contractAddress, ChainEnum chain_id)
        {
            try
            {
                // 连接到以太坊区块链网络
                var web3 = new Web3(nodeHttps);
                // 读取JSON文件内容
                string jsonFilePath = "Ebay.json"; // 替换为正确的JSON文件路径

                string jsonString = File.ReadAllText(jsonFilePath);

                // 解析JSON
                JObject jsonObject = JObject.Parse(jsonString);

                // 获取abi节点的值
                string abi = jsonObject["abi"]?.ToString();

                var contract = new Contract(new EthApiService(web3.Client), abi, contractAddress);

                StreamingWebSocketClient.ForceCompleteReadTotalMilliseconds = Timeout.Infinite;
                //StreamingWebSocketClient.ConnectionTimeout = Timeout.InfiniteTimeSpan;
                var client = new StreamingWebSocketClient(nodeWss);

                var addOrder = Event<AddOrderEventDTO>.GetEventABI().CreateFilterInput();
                var subscription = new EthLogsObservableSubscription(client);
                Action<Exception> onErrorAction = async (ex) =>
                {
                    // 处理异常情况 ex
                    Console.WriteLine($"Error AddOrder: {ex}");
                    await StartAsync(nodeWss, contractAddress, chain_id);
                };
                // attach a handler for Transfer event logs
                subscription.GetSubscriptionDataResponsesAsObservable().Subscribe(log =>
                {
                    // decode the log into a typed event log
                    var decoded = Event<AddOrderEventDTO>.DecodeEvent(log);
                    if (decoded != null && log.Address.Equals(contractAddress, StringComparison.OrdinalIgnoreCase))
                    {
                        Console.WriteLine("AddOrder监听到了！");

                        _masterDbContext.SaveChanges();
                    }
                    else
                    {
                        Console.WriteLine("CardPurchased:Found not standard log");
                    }

                }, onErrorAction);

                await client.StartAsync();

                await subscription.SubscribeAsync(addOrder);

            }
            catch (Exception ex)
            {
                await StartAsync(nodeWss, contractAddress, chain_id);
                Console.WriteLine($"AddOrder:{ex}");
                Console.WriteLine("AddOrder重启了EX");
            }
        }

    }
}

