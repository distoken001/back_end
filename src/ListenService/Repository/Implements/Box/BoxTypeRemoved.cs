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
        public BoxTypeRemoved(IConfiguration configuration, IServiceProvider serviceProvider,IDatabase redisDb)
        {
            _configuration = configuration;
            _serviceProvider = serviceProvider;
            _redisDb = redisDb;
        }


        public async Task StartAsync(string nodeUrl, string contractAddress, ChainEnum chain_id)
        {
            StreamingWebSocketClient.ForceCompleteReadTotalMilliseconds = Timeout.Infinite;
            //StreamingWebSocketClient.ConnectionTimeout = Timeout.InfiniteTimeSpan;
            var client = new StreamingWebSocketClient(nodeUrl);

            try
            {
                var cardTypeAdded = Event<BoxTypeRemovedEventDTO>.GetEventABI().CreateFilterInput();
                cardTypeAdded.Address = new string[] { contractAddress };
                var subscription = new EthLogsObservableSubscription(client);
                //Action<Exception> onErrorAction = async (ex) =>
                //{
                //    // 处理异常情况 ex
                //    client.Dispose();
                //    Console.WriteLine($"Error BoxTypeRemoved: {ex}");
                //    await StartAsync(nodeUrl, contractAddress, chain_id);
                //};
                // attach a handler for Transfer event logs
                subscription.GetSubscriptionDataResponsesAsObservable().Subscribe(log =>
                {
                    if (!_redisDb.LockTake(log.TransactionHash, 1, TimeSpan.FromSeconds(10)))
                    {
                        return;
                    }
                    Console.WriteLine("BoxTypeAdded监听到了！");
                    // decode the log into a typed event log
                    var decoded = Event<BoxTypeRemovedEventDTO>.DecodeEvent(log);
                    if (decoded != null && log.Address.Equals(contractAddress, StringComparison.OrdinalIgnoreCase))
                    {
                        using (var scope = _serviceProvider.CreateScope())
                        {
                            var _masterDbContext = scope.ServiceProvider.GetRequiredService<MySqlMasterDbContext>();
                            var card = _masterDbContext.card_type.Where(a => a.type == decoded.Event.BoxType && a.chain_id == chain_id && a.state == 1).FirstOrDefault();
                            card.state = 0;
                            _masterDbContext.SaveChanges();
                        }
                    }
                    else
                    {

                        Console.WriteLine("BoxTypeAdded: Found not standard log");
                    }
                });
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
                //        Console.WriteLine("BoxTypeRemoved重启了");
                //        break;

                //    }
                //    await Task.Delay(500);
                //}
            }
            catch (Exception ex)
            {
                //client.Dispose();
                Console.WriteLine($"BoxTypeRemoved:{ex}");
                //await StartAsync(nodeUrl, contractAddress, chain_id);
                //Console.WriteLine("BoxTypeRemoved重启了ex");

            }
        }
    }
}
