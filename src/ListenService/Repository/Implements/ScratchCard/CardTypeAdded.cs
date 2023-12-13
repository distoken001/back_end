using System;
using System.Threading.Tasks;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using Nethereum.Contracts;
using Nethereum.RPC.Eth.DTOs;
using CommonLibrary.DbContext;
using Newtonsoft.Json.Linq;
using Nethereum.JsonRpc.WebSocketClient;
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

namespace ListenService.Repository.Implements
{
    public class CardTypeAdded : ICardTypeAdded
    {
        private readonly IConfiguration _configuration;
        private readonly MySqlMasterDbContext _masterDbContext;
        public CardTypeAdded(IConfiguration configuration, MySqlMasterDbContext mySqlMasterDbContext)
        {
            _configuration = configuration;
            _masterDbContext = mySqlMasterDbContext;
        }


        public async Task StartAsync(string nodeUrl, string contractAddress, ChainEnum chain_id)
        {

            try
            {
                //// Infura 提供的以太坊节点 WebSocket 地址
                //string nodeUrl = _configuration["OP:WSS_URL"];

                //// 你的以太坊智能合约地址
                //string contractAddress = _configuration["OP:Contract_ScratchCard"];

                // 读取JSON文件内容
                string jsonFilePath = "ScratchCard.json"; // 替换为正确的JSON文件路径

                string jsonString = System.IO.File.ReadAllText(jsonFilePath);

                // 解析JSON
                JObject jsonObject = JObject.Parse(jsonString);

                // 获取abi节点的值
                string abi = jsonObject["abi"]?.ToString();
                //StreamingWebSocketClient.ForceCompleteReadTotalMilliseconds = Timeout.Infinite;
                StreamingWebSocketClient.ConnectionTimeout = Timeout.InfiniteTimeSpan;
                var client = new StreamingWebSocketClient(nodeUrl);

                var cardTypeAdded = Event<CardTypeAddedEventDTO>.GetEventABI().CreateFilterInput();
                Action<Exception> onErrorAction = async (ex) =>
                {
                    // 处理异常情况 ex
                    // 例如：
                    Console.WriteLine($"Error CardTypeAdded: {ex}");
                    await StartAsync(nodeUrl, contractAddress, chain_id);
                };
                var subscription = new EthLogsObservableSubscription(client);
                // attach a handler for Transfer event logs
                subscription.GetSubscriptionDataResponsesAsObservable().Subscribe(log =>
                {

                    // decode the log into a typed event log
                    var decoded = Event<CardTypeAddedEventDTO>.DecodeEvent(log);
                    if (decoded != null && log.Address.Equals(contractAddress, StringComparison.OrdinalIgnoreCase))
                    {
                        Console.WriteLine("CardTypeAdded监听到了！");
                        var chainToken = _masterDbContext.chain_tokens.Where(a => a.token_address.Equals(decoded.Event.TokenAddress) && a.chain_id == chain_id).FirstOrDefault();
                        var decimals_num = (double)Math.Pow(10, chainToken.decimals);
                        var cardType = new card_type() { type = decoded.Event.CardType, max_prize = (double)decoded.Event.MaxPrize / decimals_num, max_prize_probability = (int)decoded.Event.MaxPrizeProbability, name = decoded.Event.CardName, price = (double)decoded.Event.Price / decimals_num, token = decoded.Event.TokenAddress, winning_probability = (int)decoded.Event.WinningProbability, chain_id = chain_id, state = 1, create_time = DateTime.Now };
                        _masterDbContext.card_type.Add(cardType);
                        _masterDbContext.SaveChanges();
                        Console.WriteLine("Contract address: " + log.Address + " Log Transfer from:" + decoded.Event.CardName);
                    }
                    else
                    {

                        Console.WriteLine("CardTypeAdded: Found not standard log");
                    }
                }, onErrorAction);
                // open the web socket connection
                await client.StartAsync();

                // begin receiving subscription data
                // data will be received on a background thread
                await subscription.SubscribeAsync(cardTypeAdded);

                //while (true)
                //{
                //    if (client.WebSocketState == WebSocketState.Aborted)
                //    {
                //        client.Dispose();
                //        await StartAsync(nodeUrl, contractAddress, chain_id);
                //        Console.WriteLine("CardTypeAdded重启了");
                //        break;

                //    }
                //    await Task.Delay(500);
                //}

            }
            catch (Exception ex)
            {
                Console.WriteLine($"CardTypeAdded:{ex}");
                await StartAsync(nodeUrl, contractAddress, chain_id);
                Console.WriteLine("CardTypeAdded重启了EX");
            }
        }
    }
}
