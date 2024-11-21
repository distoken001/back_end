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
        private readonly ClientManage _clientManage;
        public BoxTypeRemoved(IConfiguration configuration, IServiceProvider serviceProvider,IDatabase redisDb, ClientManage clientManage)
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
                var cardTypeAdded = Event<BoxTypeRemovedEventDTO>.GetEventABI().CreateFilterInput();
                cardTypeAdded.Address = new string[] { contractAddress };
                var subscription = new EthLogsObservableSubscription(_clientManage.GetClient());
         
                subscription.GetSubscriptionDataResponsesAsObservable().Subscribe(async log =>
                {
                    try
                    {
                        if (!_redisDb.LockTake(log.TransactionHash, 1, TimeSpan.FromSeconds(10)))
                        {
                            return;
                        }
                        Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "BoxTypeAdded监听到了！");
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
                        _clientManage.GetClient().RemoveSubscription(subscription.SubscriptionId);
                        Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + $"BoxTypeAdded1:{ex}");
                        await Task.Delay(2000);
                        await StartAsync(nodeUrl, contractAddress, chain_id);
                    }
                }, async (ex) => {
                    _clientManage.GetClient().RemoveSubscription(subscription.SubscriptionId);
                    Console.WriteLine( DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")+$"BoxTypeAdded2:{ex}");
                    await Task.Delay(2000);
                    await StartAsync(nodeUrl, contractAddress, chain_id);
                });
                // begin receiving subscription data
                // data will be received on a background thread
                await subscription.SubscribeAsync(cardTypeAdded);

            }
            catch (Exception ex)
            {
                Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + $"BoxTypeRemoved3:{ex}");
                await Task.Delay(2000);
                await StartAsync(nodeUrl, contractAddress, chain_id);
            }
        }
    }
}
