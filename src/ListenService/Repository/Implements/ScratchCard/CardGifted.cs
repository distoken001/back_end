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
    public class CardGifted : ICardGifted
    {
        private readonly IConfiguration _configuration;
        private readonly MySqlMasterDbContext _masterDbContext;
        public CardGifted(IConfiguration configuration, MySqlMasterDbContext mySqlMasterDbContext)
        {
            _configuration = configuration;
            _masterDbContext = mySqlMasterDbContext;
        }
        public async Task StartAsync(string nodeUrl, string contractAddress, ChainEnum chain_id)
        {
            try
            {
                StreamingWebSocketClient.ForceCompleteReadTotalMilliseconds = Timeout.Infinite;
                //StreamingWebSocketClient.ConnectionTimeout = Timeout.InfiniteTimeSpan;
                var client = new StreamingWebSocketClient(nodeUrl);
                
                var cardGifted = Event<CardGiftedEventDTO>.GetEventABI().CreateFilterInput();
                var subscription = new EthLogsObservableSubscription(client);
                Action<Exception> onErrorAction = async (ex) =>
                {
                    // 处理异常情况 ex
                    Console.WriteLine($"Error CardGifted: {ex}");
                    await StartAsync(nodeUrl, contractAddress, chain_id);
                };
                // attach a handler for Transfer event logs
                subscription.GetSubscriptionDataResponsesAsObservable().Subscribe(log =>
                {
                    // decode the log into a typed event log
                    var decoded = Event<CardGiftedEventDTO>.DecodeEvent(log);
                    if (decoded != null && log.Address.Equals(contractAddress, StringComparison.OrdinalIgnoreCase))
                    {
                        Console.WriteLine("CardGifted监听到了！");
                        var card = _masterDbContext.card_type.Where(a => a.type == decoded.Event.CardType && a.chain_id == chain_id && a.state == 1).FirstOrDefault();
                        var token = _masterDbContext.chain_tokens.Where(a => a.token_address.Equals(card.token) && a.chain_id == card.chain_id).FirstOrDefault();
                        var cardNotOpenedSender = _masterDbContext.card_not_opened.Where(a => a.buyer.Equals(decoded.Event.Sender) && a.card_type.Equals(card.type) && a.contract.Equals(log.Address)).FirstOrDefault();

                        cardNotOpenedSender.amount -= (int)decoded.Event.NumberOfCards;
                        cardNotOpenedSender.updater = "system";
                        cardNotOpenedSender.update_time = DateTime.Now;


                        var cardNotOpenedRecipient = _masterDbContext.card_not_opened.Where(a => a.buyer.Equals(decoded.Event.Recipient) && a.card_type.Equals(card.type) && a.contract.Equals(log.Address)).FirstOrDefault();
                        if (cardNotOpenedRecipient != null)
                        {
                            cardNotOpenedRecipient.amount += (int)decoded.Event.NumberOfCards;
                            cardNotOpenedRecipient.updater = "system";
                            cardNotOpenedRecipient.update_time = DateTime.Now;
                        }
                        else
                        {
                            var notOpened = new card_not_opened() { card_type = card.type, card_name = card.name, amount = (int)decoded.Event.NumberOfCards, buyer = decoded.Event.Recipient, chain_id = chain_id, contract = log.Address, create_time = DateTime.Now, creator = "system", price = card.price, token = card.token, img = card.img };
                            _masterDbContext.card_not_opened.Add(notOpened);
                        }
                        _masterDbContext.SaveChanges();
                    }
                    else
                    {
                        Console.WriteLine("CardPurchased:Found not standard log");
                    }
                }, onErrorAction);
                // open the web socket connection
                await client.StartAsync();

                // begin receiving subscription data
                // data will be received on a background thread
                await subscription.SubscribeAsync(cardGifted);
                //while (true)
                //{
                //    if (client.WebSocketState == WebSocketState.Aborted)
                //    {
                //        client.Dispose();
                //        await StartAsync(nodeUrl, contractAddress, chain_id);
                //        Console.WriteLine("CardGifted重启了");
                //        break;

                //    }
                //    await Task.Delay(500);
                //}
            }
            catch (Exception ex)
            {
                await StartAsync(nodeUrl, contractAddress, chain_id);
                Console.WriteLine($"CardGifted:{ex}");
                Console.WriteLine("CardGifted重启了EX");
            }
        }

    }
}

