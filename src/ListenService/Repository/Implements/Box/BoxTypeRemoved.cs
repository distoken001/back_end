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
using StackExchange.Redis;

namespace ListenService.Repository.Implements
{
    public class BoxTypeRemoved : IBoxTypeRemoved
    {
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;
        private readonly IDatabase _redisDb;
        private readonly StreamingWebSocketClient _client;
        public BoxTypeRemoved(IConfiguration configuration, IServiceProvider serviceProvider,IDatabase redisDb,StreamingWebSocketClient client)
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
                var cardTypeAdded = Event<BoxTypeRemovedEventDTO>.GetEventABI().CreateFilterInput();
                cardTypeAdded.Address = new string[] { contractAddress };
                var subscription = new EthLogsObservableSubscription(_client);
         
                subscription.GetSubscriptionDataResponsesAsObservable().Subscribe(async log =>
                {
                    try
                    {
                        if (!_redisDb.LockTake(log.TransactionHash, 1, TimeSpan.FromSeconds(10)))
                        {
                            return;
                        }
                        Console.WriteLine("BoxTypeAdded监听到了！");
                        // decode the log into a typed event log
                        var decoded = Event<BoxTypeRemovedEventDTO>.DecodeEvent(log);

                        using (var scope = _serviceProvider.CreateScope())
                        {
                            var _masterDbContext = scope.ServiceProvider.GetRequiredService<MySqlMasterDbContext>();
                            var card = _masterDbContext.card_type.Where(a => a.type == decoded.Event.BoxType && a.chain_id == chain_id && a.state == 1).FirstOrDefault();
                            card.state = 0;
                            _masterDbContext.SaveChanges();
                        }
                    }
                    catch(Exception ex)
                    {
                        await subscription.UnsubscribeAsync();
                        Console.WriteLine($"BoxTypeAdded:{ex}");
                        await Task.Delay(2000);
                        await StartAsync(nodeUrl, contractAddress, chain_id);
                    }
                }, async (ex) => {
                    await subscription.UnsubscribeAsync();
                    Console.WriteLine($"BoxTypeAdded:{ex}");
                    await Task.Delay(2000);
                    await StartAsync(nodeUrl, contractAddress, chain_id);
                });
                // begin receiving subscription data
                // data will be received on a background thread
                await subscription.SubscribeAsync(cardTypeAdded);

            }
            catch (Exception ex)
            {
                Console.WriteLine($"BoxTypeRemoved:{ex}");
                await Task.Delay(2000);
                await StartAsync(nodeUrl, contractAddress, chain_id);
            }
        }
    }
}
