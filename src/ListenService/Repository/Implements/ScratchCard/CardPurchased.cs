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

namespace ListenService.Repository.Implements
{
    public class CardPurchased : ICardPurchased
    {
        private readonly IConfiguration _configuration;
        private readonly MySqlMasterDbContext _masterDbContext;
        private StreamingWebSocketClient _client;
        private EthLogsObservableSubscription _subscription;
        public CardPurchased(IConfiguration configuration, MySqlMasterDbContext mySqlMasterDbContext)
        {
            _configuration = configuration;
            _masterDbContext = mySqlMasterDbContext;
        }
        public async Task StartAsync(string nodeUrl, string contractAddress, ChainEnum chain_id)
        {
            try
            {
                _client = new StreamingWebSocketClient(nodeUrl);

                var cardPurchased = Event<CardPurchasedEventDTO>.GetEventABI().CreateFilterInput();

                _subscription = new EthLogsObservableSubscription(_client);
                // attach a handler for Transfer event logs
                _subscription.GetSubscriptionDataResponsesAsObservable().Subscribe(log =>
                {
                    try
                    {
                        Console.WriteLine("监听到了！");
                        // decode the log into a typed event log
                        var decoded = Event<CardPurchasedEventDTO>.DecodeEvent(log);
                        if (decoded != null && log.Address.Equals(contractAddress, StringComparison.OrdinalIgnoreCase))
                        {
                            var card = _masterDbContext.card_type.Where(a => a.type == decoded.Event.CardType && a.chain_id == chain_id).FirstOrDefault();
                            var token = _masterDbContext.chain_tokens.Where(a => a.token_address.Equals(card.token) && a.chain_id == card.chain_id).FirstOrDefault();
                            var cardNotOpened = _masterDbContext.card_not_opened.Where(a => a.buyer.Equals(decoded.Event.User) && a.card_type.Equals(card.type) && a.contract.Equals(log.Address) && a.token.Equals(token.token_address)).FirstOrDefault();
                            if (cardNotOpened != null)
                            {
                                cardNotOpened.amount += (int)decoded.Event.NumberOfCards;
                                cardNotOpened.updater = "system";
                                cardNotOpened.update_time = DateTime.Now;
                            }
                            else
                            {
                                var notOpened = new card_not_opened() { card_type = card.type, card_name = card.name, amount = (int)decoded.Event.NumberOfCards, buyer = decoded.Event.User, chain_id = chain_id, contract = log.Address, create_time = DateTime.Now, creator = "system", price = card.price, token = card.token, img = card.img };
                                _masterDbContext.card_not_opened.Add(notOpened);
                            }
                            _masterDbContext.SaveChanges();
                        }
                        else
                        {

                            Console.WriteLine("CardPurchased:Found not standard log");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("CardPurchased:Log Address: " + log.Address + " is not a standard log:", ex.Message);
                    }
                });
                // open the web socket connection
                await _client.StartAsync();

                // begin receiving subscription data
                // data will be received on a background thread
                await _subscription.SubscribeAsync(cardPurchased);
            }
            catch (Exception ex)
            {
                if (_subscription != null)
                {
                    await _subscription.UnsubscribeAsync();
                    await _client.StopAsync();
                }

                //// 延迟一段时间后尝试重新建立连接
                //await Task.Delay(TimeSpan.FromSeconds(1)); // 这里设置重连间隔时间，根据需要调整

                await StartAsync(nodeUrl, contractAddress, chain_id);
                Console.WriteLine($"WebSocket Error:{ex}");
            }
        }

    }
}

