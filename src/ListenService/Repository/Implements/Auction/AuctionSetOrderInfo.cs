using CommonLibrary.Common.Common;
using CommonLibrary.DbContext;
using ListenService.Model;
using ListenService.Repository.Interfaces;
using Nethereum.Contracts;
using Nethereum.JsonRpc.WebSocketStreamingClient;
using Nethereum.RPC;
using Nethereum.RPC.Reactive.Eth.Subscriptions;
using Nethereum.Util;
using Nethereum.Web3;
using Newtonsoft.Json.Linq;
using System.Reactive.Linq;

namespace ListenService.Repository.Implements
{
    public class AuctionSetOrderInfo : IAuctionSetOrderInfo
    {
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;
        private readonly ISendMessage _sendMessage;

        public AuctionSetOrderInfo(IConfiguration configuration, IServiceProvider serviceProvider, ISendMessage sendMessage)
        {
            _configuration = configuration;
            _serviceProvider = serviceProvider;
            _sendMessage = sendMessage;
        }

        public async Task StartAsync(string nodeWss, string nodeHttps, string contractAddress, ChainEnum chain_id)
        {
            StreamingWebSocketClient.ForceCompleteReadTotalMilliseconds = Timeout.Infinite;
            //StreamingWebSocketClient.ConnectionTimeout = Timeout.InfiniteTimeSpan;
            var client = new StreamingWebSocketClient(nodeWss);
            try
            {
                // 连接到以太坊区块链网络
                var web3 = new Web3(nodeHttps);
                // 读取JSON文件内容
                string jsonFilePath = "Auction.json"; // 替换为正确的JSON文件路径

                string jsonString = File.ReadAllText(jsonFilePath);

                // 解析JSON
                JObject jsonObject = JObject.Parse(jsonString);

                // 获取abi节点的值
                string abi = jsonObject["abi"]?.ToString();

                var contract = new Contract(new EthApiService(web3.Client), abi, contractAddress);
                var function = contract.GetFunction("orders");
                var function2 = contract.GetFunction("orderTime");
                var function3 = contract.GetFunction("orderBidCount");
                var addOrder = Event<AuctionSetOrderInfoEventDTO>.GetEventABI().CreateFilterInput();
                var subscription = new EthLogsObservableSubscription(client);

                Action<Exception> onErrorAction = async (ex) =>
                {
                    // 处理异常情况 ex
                    Console.WriteLine($"Error AuctionSetOrderInfo: {ex}");
                    client.Dispose();
                    await StartAsync(nodeWss, nodeHttps, contractAddress, chain_id);
                };
                // attach a handler for Transfer event logs
                subscription.GetSubscriptionDataResponsesAsObservable().Subscribe(async log =>
                {
                    // decode the log into a typed event log
                    var decoded = Event<AuctionSetOrderInfoEventDTO>.DecodeEvent(log);
                    if (decoded != null && log.Address.Equals(contractAddress, StringComparison.OrdinalIgnoreCase))
                    {
                        using (var scope = _serviceProvider.CreateScope())
                        {
                            var _masterDbContext = scope.ServiceProvider.GetRequiredService<MySqlMasterDbContext>();
                            Console.WriteLine("AuctionSetOrderInfo监听到了！");
                            // 调用智能合约函数并获取返回结果
                            var orderResult = await function.CallDeserializingToObjectAsync<AuctionOrderDTO>((int)decoded.Event.OrderId);
                            var dateTime = await function2.CallDeserializingToObjectAsync<AuctionDateTimeDTO>((int)decoded.Event.OrderId);
                            var bidCount = await function3.CallAsync<int>((int)decoded.Event.OrderId);
                            var chainToken = _masterDbContext.chain_tokens.Where(a => a.token_address.Equals(orderResult.Token) && a.chain_id == chain_id).FirstOrDefault();
                            var decimals_num = new BigDecimal(Math.Pow(10, chainToken.decimals));

                            var order = _masterDbContext.orders_auction.Where(a => a.order_id == (int)decoded.Event.OrderId && a.chain_id == chain_id && a.contract.Equals(contractAddress)).FirstOrDefault();

                            order.status = orderResult.Status;
                            order.buyer_ex = (double)(new BigDecimal(orderResult.BuyerEx) / decimals_num);
                            order.update_time = DateTime.Now;
                            order.buyer = orderResult.Buyer;
                            order.buyer_pledge = (double)(new BigDecimal(orderResult.BuyerPledge) / decimals_num);
                            order.seller_pledge = (double)(new BigDecimal(orderResult.SellerPledge) / decimals_num);
                            order.amount = (double)orderResult.Amount;
                            order.price = (double)(new BigDecimal(orderResult.Price) / decimals_num);
                            order.end_time = (long)dateTime.EndTime;
                            order.count = bidCount;
                            order.updater = "system";

                            _masterDbContext.SaveChanges();
                            _ = _sendMessage.SendMessageAuction((int)decoded.Event.OrderId, chain_id, contractAddress);
                        }
                    }
                    else
                    {
                        Console.WriteLine("AuctionSetOrderInfo:Found not standard log");
                    }
                }, onErrorAction);

                await client.StartAsync();

                await subscription.SubscribeAsync(addOrder);
            }
            catch (Exception ex)
            {
                client.Dispose();
                await StartAsync(nodeWss, nodeHttps, contractAddress, chain_id);
                Console.WriteLine($"AuctionSetOrderInfo:{ex}");
                Console.WriteLine("AuctionSetOrderInfo重启了EX");
            }
        }
    }
}