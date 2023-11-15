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
using ListenWeb3.Model;
using Nethereum.JsonRpc.Client;
using CommonLibrary.Model.DataEntityModel;
using CommonLibrary.Common.Common;

namespace ListenWeb3.Service.ScratchCard
{
    public class CardPurchasedService : IHostedService
    {
        private readonly IConfiguration _configuration;
        private readonly MySqlMasterDbContext _masterDbContext;
        public CardPurchasedService(IConfiguration configuration, MySqlMasterDbContext mySqlMasterDbContext)
        {
            _configuration = configuration;
            _masterDbContext = mySqlMasterDbContext;
        }
        public async Task StartAsync(CancellationToken cancellationToken)
        {

            try
            {
                // Infura 提供的以太坊节点 WebSocket 地址
                string nodeUrl = _configuration["OP:WSS_URL"];

                // 你的以太坊智能合约地址
                string contractAddress = _configuration["OP:Contract_ScratchCard"];

                var client = new StreamingWebSocketClient(nodeUrl);

                var cardPurchased = Event<CardPurchasedEventDTO>.GetEventABI().CreateFilterInput();

                var subscription = new EthLogsObservableSubscription(client);
                // attach a handler for Transfer event logs
                subscription.GetSubscriptionDataResponsesAsObservable().Subscribe(log =>
                {
                    try
                    {
                        // decode the log into a typed event log
                        var decoded = Event<CardPurchasedEventDTO>.DecodeEvent(log);
                        if (decoded != null)
                        {
                            ChainEnum chain_id = ChainEnum.OptimisticGoerli;
                            if (_configuration["Env"] == "prod")
                            {
                                chain_id = ChainEnum.Optimism;
                            }
                            var card = _masterDbContext.card_type.Where(a => a.type == decoded.Event.CardType).FirstOrDefault();
                            if (card != null)
                            {
                                var cardNotOpened = _masterDbContext.card_not_opened.Where(a => a.buyer.Equals(decoded.Event.User) && a.card_type.Equals(card.type) && a.contract.Equals(a.contract)).FirstOrDefault();
                                if (cardNotOpened != null)
                                {
                                    cardNotOpened.amount += (int)decoded.Event.NumberOfCards;
                                    cardNotOpened.updater = "system";
                                    cardNotOpened.update_time = DateTime.Now;
                                }
                                else
                                {
                                    var notOpened = new card_not_opened() { card_type = card.type, card_name = card.name, amount = (int)decoded.Event.NumberOfCards, buyer = decoded.Event.User, chain_id = chain_id, contract = log.Address, create_time = DateTime.Now, creator = "system", price = card.price };
                                    _masterDbContext.card_not_opened.Add(notOpened);
                                }
                                _masterDbContext.SaveChanges();
                            }
                            else
                            {
                                return;
                            }
                        }
                        else
                        {

                            Console.WriteLine("Found not standard CardPurchasedEvent log");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Log Address: " + log.Address + " is not a standard log:", ex.Message);
                    }
                });
                // open the web socket connection
                await client.StartAsync();

                // begin receiving subscription data
                // data will be received on a background thread
                await subscription.SubscribeAsync(cardPurchased);


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
        public Task StopAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

    }
}

