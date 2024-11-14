using CommonLibrary.Common.Common;
using CommonLibrary.DbContext;
using CommonLibrary.Model.DataEntityModel;
using ListenService.Chains;
using ListenService.Model;
using ListenService.Repository.Interfaces;
using Nethereum.Contracts;
using Nethereum.JsonRpc.WebSocketStreamingClient;
using Nethereum.RPC.Reactive;
using Nethereum.RPC.Reactive.Eth.Subscriptions;
using StackExchange.Redis;
using System.Net.WebSockets;
using System.Reactive.Linq;

namespace ListenService.Repository.Implements
{
    public class BoxMinted : IBoxMinted
    {
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;
        private readonly IDatabase _redisDb;
        private readonly StreamingWebSocketClient _client;
        public BoxMinted(IConfiguration configuration, IServiceProvider serviceProvider,IDatabase redisDb,StreamingWebSocketClient client)
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
                for (int i = 0; i < 10; i++)
                {
                    if (_client.WebSocketState == WebSocketState.Open)
                    {
                        break;
                    }
                    else
                    {
                        await Task.Delay(500).ConfigureAwait(false);
                    }
                }
                var _subscription = new EthLogsObservableSubscription(_client);
                var cardPurchased = Event<BoxMintedEventDTO>.GetEventABI().CreateFilterInput();
                cardPurchased.Address = new string[] { contractAddress };
                //Action<Exception> onErrorAction = async (ex) =>
                //{
                //    // 处理异常情况 ex
                //    // 例如：
                //    Console.WriteLine($"Error BoxPurchased: {ex}");
                //    _client.Dispose();
                //    await StartAsync(nodeUrl, contractAddress, chain_id);
                //};
                _subscription.GetSubscriptionDataResponsesAsObservable().Subscribe(async log =>
                {
                    try
                    {
                        if (!_redisDb.LockTake(log.TransactionHash, 1, TimeSpan.FromSeconds(10)))
                        {
                            return;
                        }
                        Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "BoxMinted监听到了！");
                        // decode the log into a typed event log
                        var decoded = Event<BoxMintedEventDTO>.DecodeEvent(log);
                        using (var scope = _serviceProvider.CreateScope())
                        {
                            var _masterDbContext = scope.ServiceProvider.GetRequiredService<MySqlMasterDbContext>();
                            var card = _masterDbContext.card_type.Where(a => a.type == decoded.Event.BoxType && a.chain_id == chain_id && a.state == 1).FirstOrDefault();
                            var token = _masterDbContext.chain_tokens.Where(a => a.token_address.Equals(card.token) && a.chain_id == card.chain_id).FirstOrDefault();
                            var cardNotOpened = _masterDbContext.card_not_opened.Where(a => a.buyer.Equals(decoded.Event.User) && a.card_type.Equals(card.type) && a.contract.Equals(log.Address) && a.token.Equals(token.token_address)).FirstOrDefault();
                            if (cardNotOpened != null)
                            {
                                cardNotOpened.amount += (int)decoded.Event.NumberOfBoxs;
                                cardNotOpened.updater = "system";
                                cardNotOpened.update_time = DateTime.Now;
                            }
                            else
                            {
                                var notOpened = new card_not_opened() { card_type = card.type, card_name = card.name, amount = (int)decoded.Event.NumberOfBoxs, buyer = decoded.Event.User, chain_id = chain_id, contract = log.Address, create_time = DateTime.Now, creator = "system", price = card.price, token = card.token, img = card.img };
                                _masterDbContext.card_not_opened.Add(notOpened);
                            }
                            _masterDbContext.SaveChanges();
                        }
                    }
                    catch(Exception ex)
                    {
                        _client.RemoveSubscription(_subscription.SubscriptionId);
                        Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + $"BoxMinted1:{ex}");
                        await Task.Delay(2000);
                        await StartAsync(nodeUrl, contractAddress, chain_id);
                    }
                     
                    
                }, async(ex) => {
                    _client.RemoveSubscription(_subscription.SubscriptionId);
                    Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + $"BoxMinted2:{ex}");
                    await Task.Delay(2000);
                    await StartAsync(nodeUrl, contractAddress, chain_id);
                });
            
                await _subscription.SubscribeAsync(cardPurchased);
            }

            catch (Exception ex)
            {
                Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + $"BoxMinted3:{ex}");
                await Task.Delay(2000);
                await StartAsync(nodeUrl, contractAddress, chain_id);
            }

        }

    }
}

