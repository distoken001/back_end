using CommonLibrary.Common.Common;
using CommonLibrary.DbContext;
using CommonLibrary.Model.DataEntityModel;
using ListenService.Model;
using ListenService.Repository.Interfaces;
using Nethereum.Contracts;
using Nethereum.JsonRpc.WebSocketStreamingClient;
using Nethereum.RPC.Reactive;
using Nethereum.RPC.Reactive.Eth.Subscriptions;
using System.Net.WebSockets;
using System.Reactive.Linq;

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
                //StreamingWebSocketClient.ForceCompleteReadTotalMilliseconds = Timeout.Infinite;
                StreamingWebSocketClient.ConnectionTimeout = Timeout.InfiniteTimeSpan;
                var _client = new StreamingWebSocketClient(nodeUrl);

                var _subscription = new EthLogsObservableSubscription(_client);
                var cardPurchased = Event<CardPurchasedEventDTO>.GetEventABI().CreateFilterInput();
                Action<Exception> onErrorAction = async (ex) =>
                {
                    // 处理异常情况 ex
                    // 例如：
                    Console.WriteLine($"Error CardPurchased: {ex}");
                    await StartAsync(nodeUrl, contractAddress, chain_id);
                };
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
                }, onErrorAction);
                await _client.StartAsync();
                await _subscription.SubscribeAsync(cardPurchased);
                //while (true)
                //{
                //    if (_client.WebSocketState == WebSocketState.Aborted)
                //    {
                //        _client.Dispose();
                //        await StartAsync(nodeUrl, contractAddress, chain_id);
                //        Console.WriteLine("CardPurchased重启了");
                //        break;

                //    }
                //    await Task.Delay(500);
                //}
            }

            catch (Exception ex)
            {
                await StartAsync(nodeUrl, contractAddress, chain_id);
                Console.WriteLine($"CardPurchased:{ex}");
                Console.WriteLine("CardPurchased重启了EX");
            }
        }

    }
}

