using Nethereum.Contracts;
using CommonLibrary.DbContext;
using Nethereum.JsonRpc.WebSocketStreamingClient;
using Nethereum.RPC.Reactive.Eth.Subscriptions;
using System.Reactive.Linq;
using ListenService.Model;
using CommonLibrary.Model.DataEntityModel;
using CommonLibrary.Common.Common;
using ListenService.Repository.Interfaces;

namespace ListenService.Repository.Implements
{
    public class BoxGifted : IBoxGifted
    {
        private readonly IConfiguration _configuration;
        private readonly MySqlMasterDbContext _masterDbContext;
        public BoxGifted(IConfiguration configuration, MySqlMasterDbContext mySqlMasterDbContext)
        {
            _configuration = configuration;
            _masterDbContext = mySqlMasterDbContext;
        }
        public async Task StartAsync(string nodeUrl, string contractAddress, ChainEnum chain_id)
        {
            StreamingWebSocketClient.ForceCompleteReadTotalMilliseconds = Timeout.Infinite;
            //StreamingWebSocketClient.ConnectionTimeout = Timeout.InfiniteTimeSpan;
            var client = new StreamingWebSocketClient(nodeUrl);
            try
            {
                var cardGifted = Event<BoxGiftedEventDTO>.GetEventABI().CreateFilterInput();
                var subscription = new EthLogsObservableSubscription(client);
                Action<Exception> onErrorAction = async (ex) =>
                {
                    // 处理异常情况 ex
                    Console.WriteLine($"Error BoxGifted: {ex}");
                    client.Dispose();
                    await StartAsync(nodeUrl, contractAddress, chain_id);
                };
                // attach a handler for Transfer event logs
                subscription.GetSubscriptionDataResponsesAsObservable().Subscribe(log =>
                {
                    // decode the log into a typed event log
                    var decoded = Event<BoxGiftedEventDTO>.DecodeEvent(log);
                    if (decoded != null && log.Address.Equals(contractAddress, StringComparison.OrdinalIgnoreCase))
                    {
                        Console.WriteLine("BoxGifted监听到了！");
                        var card = _masterDbContext.card_type.Where(a => a.type == decoded.Event.BoxType && a.chain_id == chain_id && a.state == 1).FirstOrDefault();
                        var token = _masterDbContext.chain_tokens.Where(a => a.token_address.Equals(card.token) && a.chain_id == card.chain_id).FirstOrDefault();
                        var cardNotOpenedSender = _masterDbContext.card_not_opened.Where(a => a.buyer.Equals(decoded.Event.Sender) && a.card_type.Equals(card.type) && a.contract.Equals(log.Address)).FirstOrDefault();

                        cardNotOpenedSender.amount -= (int)decoded.Event.NumberOfBoxs;
                        cardNotOpenedSender.updater = "system";
                        cardNotOpenedSender.update_time = DateTime.Now;


                        var cardNotOpenedRecipient = _masterDbContext.card_not_opened.Where(a => a.buyer.Equals(decoded.Event.Recipient) && a.card_type.Equals(card.type) && a.contract.Equals(log.Address)).FirstOrDefault();
                        if (cardNotOpenedRecipient != null)
                        {
                            cardNotOpenedRecipient.amount += (int)decoded.Event.NumberOfBoxs;
                            cardNotOpenedRecipient.updater = "system";
                            cardNotOpenedRecipient.update_time = DateTime.Now;
                        }
                        else
                        {
                            var notOpened = new card_not_opened() { card_type = card.type, card_name = card.name, amount = (int)decoded.Event.NumberOfBoxs, buyer = decoded.Event.Recipient, chain_id = chain_id, contract = log.Address, create_time = DateTime.Now, creator = "system", price = card.price, token = card.token, img = card.img };
                            _masterDbContext.card_not_opened.Add(notOpened);
                        }
                        _masterDbContext.SaveChanges();
                    }
                    else
                    {
                        Console.WriteLine("BoxPurchased:Found not standard log");
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
                //        Console.WriteLine("BoxGifted重启了");
                //        break;

                //    }
                //    await Task.Delay(500);
                //}
            }
            catch (Exception ex)
            {
                client.Dispose();
                await StartAsync(nodeUrl, contractAddress, chain_id);
                Console.WriteLine($"BoxGifted:{ex}");
                Console.WriteLine("BoxGifted重启了EX");
            }
        }

    }
}

