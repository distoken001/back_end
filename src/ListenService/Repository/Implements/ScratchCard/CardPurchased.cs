using CommonLibrary.Common.Common;
using CommonLibrary.DbContext;
using CommonLibrary.Model.DataEntityModel;
using ListenService.Model;
using ListenService.Repository.Interfaces;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;
using Nethereum.JsonRpc.Client.Streaming;
using Nethereum.JsonRpc.WebSocketStreamingClient;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.RPC.Eth.Subscriptions;
using Nethereum.RPC.Reactive.Eth.Subscriptions;
using Newtonsoft.Json;
using System;
using System.Numerics;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace ListenService.Repository.Implements
{
    public class CardPurchased : ICardPurchased
    {
        private readonly IConfiguration _configuration;
        private readonly MySqlMasterDbContext _masterDbContext;
        public CardPurchased(IConfiguration configuration, MySqlMasterDbContext mySqlMasterDbContext)
        {
            _configuration = configuration;
            _masterDbContext = mySqlMasterDbContext;
        }
        public async Task StartAsync(string nodeUrl, string contractAddress, ChainEnum chain_id)
        {
            try
            {
                using (var _client = new StreamingWebSocketClient(nodeUrl))
                {
                    try
                    {
                        var _subscription = new EthLogsObservableSubscription(_client);
                        var cardPurchased = Event<CardPurchasedEventDTO>.GetEventABI().CreateFilterInput();

                        _subscription.GetSubscriptionDataResponsesAsObservable().Subscribe(log =>
                        {
                            Console.WriteLine("CardPurchased监听到了！");
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
                        });
                        await _client.StartAsync();
                        await _subscription.SubscribeAsync(cardPurchased);

                        // 在这里保持连接处于活动状态，可以根据需要添加逻辑
                        await Task.Delay(Timeout.Infinite);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"WebSocket Error: {ex}");
                    }
                }
            }
            catch (Exception ex)
            {
                await StartAsync(nodeUrl, contractAddress, chain_id);
                Console.WriteLine($"WebSocket Error:{ex}");
            }
        }

    }
}

