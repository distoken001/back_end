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
using Microsoft.VisualBasic;
using Nethereum.Contracts.Standards.ERC20.TokenList;
using ListenService.Repository.Interfaces;
using System.Net.WebSockets;
using StackExchange.Redis;

namespace ListenService.Repository.Implements
{
    public class PrizeClaimed : IPrizeClaimed
    {
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;
        private readonly IDatabase _redisDb;
        private readonly StreamingWebSocketClient _client;
        public PrizeClaimed(IConfiguration configuration, IServiceProvider serviceProvider,IDatabase redisDb,StreamingWebSocketClient client)
        {
            _configuration = configuration;
            _serviceProvider = serviceProvider;
            _redisDb = redisDb;
            _client = client;
        }
        public async Task StartAsync(string nodeUrl, string contractAddress, ChainEnum chain_id)
        {
           
            try
            {
                await _client.StartAsync();
                //// Infura 提供的以太坊节点 WebSocket 地址
                //string nodeUrl = _configuration["OP:WSS_URL"];

                //// 你的以太坊智能合约地址

                var prizeClaimed = Event<PrizeClaimedEventDTO>.GetEventABI().CreateFilterInput();
                prizeClaimed.Address = new string[] { contractAddress };
                var subscription = new EthLogsObservableSubscription(_client);
                         
                subscription.GetSubscriptionDataResponsesAsObservable().Subscribe( async log =>
                {
                    try
                    {
                        if (!_redisDb.LockTake(log.TransactionHash, 1, TimeSpan.FromSeconds(10)))
                        {
                            return;
                        }
                        Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "PrizeClaimed监听到了！");
                        // decode the log into a typed event log
                        var decoded = Event<PrizeClaimedEventDTO>.DecodeEvent(log);
                        using (var scope = _serviceProvider.CreateScope())
                        {
                            var _masterDbContext = scope.ServiceProvider.GetRequiredService<MySqlMasterDbContext>();
                            var card = _masterDbContext.card_type.Where(a => a.type == decoded.Event.BoxType && a.chain_id == chain_id && a.state == 1).FirstOrDefault();
                            var chainToken = _masterDbContext.chain_tokens.Where(a => a.token_address.Equals(card.token) && a.chain_id == chain_id).FirstOrDefault();
                            var decimals_num = (double)Math.Pow(10, chainToken.decimals);
                            var prize = (double)decoded.Event.Prize / decimals_num;
                            _masterDbContext.card_opened.Add(new card_opened() { buyer = decoded.Event.User, card_name = card.name, card_type = card.type, chain_id = chain_id, create_time = DateTime.Now, creator = "system", contract = log.Address, img = prize > 0 ? card.img_win : card.img_fail, price = card.price, token = card.token, wining = prize });
                            var cardNotOpened = _masterDbContext.card_not_opened.Where(a => a.buyer.Equals(decoded.Event.User) && a.card_type.Equals(card.type) && a.contract.Equals(log.Address) && a.token.Equals(chainToken.token_address)).FirstOrDefault();
                            cardNotOpened.amount -= 1;
                            cardNotOpened.updater = "system";
                            cardNotOpened.update_time = DateTime.Now;

                            _masterDbContext.SaveChanges();
                        }
                    }
                    catch(Exception ex)
                    {
                        _client.RemoveSubscription(subscription.SubscriptionId);
                        Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + $"PrizeClaimed:{ex}");
                        await Task.Delay(2000);
                        await StartAsync(nodeUrl, contractAddress, chain_id);
                    }

                }, async (ex) => {
                    _client.RemoveSubscription(subscription.SubscriptionId);
                    Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + $"PrizeClaimed:{ex}");
                    await Task.Delay(2000);
                    await StartAsync(nodeUrl, contractAddress, chain_id);
                });

                // begin receiving subscription data
                // data will be received on a background thread
                await subscription.SubscribeAsync(prizeClaimed);
                //while (true)
                //{
                //    if (client.WebSocketState == WebSocketState.Aborted)
                //    {
                //        client.Dispose();
                //        await StartAsync(nodeUrl, contractAddress, chain_id);
                //        Console.WriteLine("PrizeClaimed重启了");
                //        break;

                //    }
                //    await Task.Delay(500);
                //}
            }
            catch (Exception ex)
            {
                Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + $"PrizeClaimed:{ex}");
                await Task.Delay(2000);
                await StartAsync(nodeUrl, contractAddress, chain_id);
              
            }
        }
    }
}

