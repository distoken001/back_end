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
    public class CardTypeRemoved : ICardTypeRemoved
    {
        private readonly IConfiguration _configuration;
        private readonly MySqlMasterDbContext _masterDbContext;
        public CardTypeRemoved(IConfiguration configuration, MySqlMasterDbContext mySqlMasterDbContext)
        {
            _configuration = configuration;
            _masterDbContext = mySqlMasterDbContext;
        }


        public async Task StartAsync(string nodeUrl, string contractAddress, ChainEnum chain_id)
        {

            try
            {

                var client = new StreamingWebSocketClient(nodeUrl);

                var cardTypeAdded = Event<CardTypeRemovedEventDTO>.GetEventABI().CreateFilterInput();

                var subscription = new EthLogsObservableSubscription(client);
                // attach a handler for Transfer event logs
                subscription.GetSubscriptionDataResponsesAsObservable().Subscribe(log =>
                {
                    try
                    {
                        // decode the log into a typed event log
                        var decoded = Event<CardTypeRemovedEventDTO>.DecodeEvent(log);
                        if (decoded != null && log.Address.Equals(contractAddress, StringComparison.OrdinalIgnoreCase))
                        {
                            var card = _masterDbContext.card_type.Where(a => a.type == decoded.Event.CardType && a.chain_id == chain_id).FirstOrDefault();
                            card.state = 0;
                            _masterDbContext.SaveChanges();
                        }
                        else
                        {

                            Console.WriteLine("CardTypeAdded: Found not standard log");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("CardTypeAdded:Log Address: " + log.Address + " is not a standard log:", ex.Message);
                    }
                });
                // open the web socket connection
                await client.StartAsync();

                // begin receiving subscription data
                // data will be received on a background thread
                await subscription.SubscribeAsync(cardTypeAdded);

                while (true)
                {
                    if (client.WebSocketState == WebSocketState.Aborted)
                    {

                        await subscription.UnsubscribeAsync();
                        client.Dispose();
                        await StartAsync(nodeUrl, contractAddress, chain_id);
                        Console.WriteLine("我重启了");
                        break;

                    }
                    await Task.Delay(1000);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
