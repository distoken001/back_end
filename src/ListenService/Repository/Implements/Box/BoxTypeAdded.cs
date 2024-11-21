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
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace ListenService.Repository.Implements
{
    public class BoxTypeAdded : IBoxTypeAdded
    {
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;
        private readonly IDatabase _redisDb;
        private readonly ClientManage _clientManage;
        public BoxTypeAdded(IConfiguration configuration, IServiceProvider serviceProvider,IDatabase redisDb, ClientManage clientManage)
        {
            _configuration = configuration;
            _serviceProvider = serviceProvider;
            _redisDb = redisDb;
            _clientManage = clientManage;
        }


        public async Task StartAsync(string nodeUrl, string contractAddress, ChainEnum chain_id)
        {
            
            try
            {
                while (true)
                {
                    if (_clientManage.GetClient().WebSocketState == WebSocketState.Open)
                    {
                        break;
                    }
                    else
                    {
                        await Task.Delay(500).ConfigureAwait(false);
                    }
                }
                //// Infura 提供的以太坊节点 WebSocket 地址
                //string nodeUrl = _configuration["OP:WSS_URL"];

                //// 你的以太坊智能合约地址
                //string contractAddress = _configuration["OP:Contract_Box"];

                // 读取JSON文件内容
                string jsonFilePath = "DeMarketBox.json"; // 替换为正确的JSON文件路径

                string jsonString = System.IO.File.ReadAllText(jsonFilePath);

                // 解析JSON
                JObject jsonObject = JObject.Parse(jsonString);

                // 获取abi节点的值
                string abi = jsonObject["abi"]?.ToString();


                var cardTypeAdded = Event<BoxTypeAddedEventDTO>.GetEventABI().CreateFilterInput();
                cardTypeAdded.Address = new string[] { contractAddress };
                var subscription = new EthLogsObservableSubscription(_clientManage.GetClient());
                // attach a handler for Transfer event logs
                subscription.GetSubscriptionDataResponsesAsObservable().Subscribe(async log =>
                {
                    try { 
                    if (!_redisDb.LockTake(log.TransactionHash, 1, TimeSpan.FromSeconds(10)))
                    {
                        return;
                    }
                    // decode the log into a typed event log
                    var decoded = Event<BoxTypeAddedEventDTO>.DecodeEvent(log);
                  
                        using (var scope = _serviceProvider.CreateScope())
                        {
                            var _masterDbContext = scope.ServiceProvider.GetRequiredService<MySqlMasterDbContext>();
                            Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "BoxTypeAdded监听到了！");
                            var chainToken = _masterDbContext.chain_tokens.Where(a => a.token_address.Equals(decoded.Event.TokenAddress) && a.chain_id == chain_id).FirstOrDefault();
                            var decimals_num = (double)Math.Pow(10, chainToken.decimals);
                            var cardType = new card_type() { type = decoded.Event.BoxType, max_prize = (double)decoded.Event.MaxPrize / decimals_num, max_prize_probability = (int)decoded.Event.MaxPrizeProbability, name = decoded.Event.BoxName, price = (double)decoded.Event.Price / decimals_num, token = decoded.Event.TokenAddress, winning_probability = (int)decoded.Event.WinningProbability, chain_id = chain_id, state = 1, create_time = DateTime.Now };
                            _masterDbContext.card_type.Add(cardType);
                            _masterDbContext.SaveChanges();
                            Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "Contract address: " + log.Address + " Log Transfer from:" + decoded.Event.BoxName);
                        }
                    }
                    catch(Exception ex)
                    {
                        _clientManage.GetClient().RemoveSubscription(subscription.SubscriptionId);
                        Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + $"BoxTypeAdded1:{ex}");
                        await Task.Delay(2000);
                        await StartAsync(nodeUrl, contractAddress, chain_id);
                    }
                 
                }, async (ex) => {
                    _clientManage.GetClient().RemoveSubscription(subscription.SubscriptionId);
                    Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + $"BoxTypeAdded2:{ex}");
                    await Task.Delay(2000);
                    await StartAsync(nodeUrl, contractAddress, chain_id);
                });
             
                await subscription.SubscribeAsync(cardTypeAdded);

            }
            catch (Exception ex)
            {
                Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + $"BoxTypeAdded3:{ex}");
                await Task.Delay(2000);
                await StartAsync(nodeUrl, contractAddress, chain_id);
            }
        }
    }
}
