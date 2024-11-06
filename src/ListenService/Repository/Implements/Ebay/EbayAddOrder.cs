using Nethereum.Contracts;
using CommonLibrary.DbContext;
using Nethereum.JsonRpc.WebSocketStreamingClient;
using Nethereum.RPC.Reactive.Eth.Subscriptions;
using Newtonsoft.Json;
using System.Reactive.Linq;
using ListenService.Model;
using CommonLibrary.Model.DataEntityModel;
using CommonLibrary.Common.Common;
using ListenService.Repository.Interfaces;
using Newtonsoft.Json.Linq;
using Nethereum.Web3;
using Nethereum.RPC;
using Nethereum.Util;
using StackExchange.Redis;

namespace ListenService.Repository.Implements
{
    public class EbayAddOrder : IEbayAddOrder
    {
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;
        private readonly ISendMessage _sendMessage;
        private readonly IDatabase _redisDb;
        public EbayAddOrder(IConfiguration configuration, IServiceProvider serviceProvider, ISendMessage sendMessage,IDatabase redisDb)
        {
            _redisDb = redisDb;
            _configuration = configuration;
            _serviceProvider = serviceProvider;
            _sendMessage = sendMessage;
        }
        public async Task StartAsync(string nodeWss, string nodeHttps, string contractAddress, ChainEnum chain_id)
        {
            StreamingWebSocketClient.ForceCompleteReadTotalMilliseconds = Timeout.Infinite;
            var client = new StreamingWebSocketClient(nodeWss);
            Console.WriteLine("EbayAddOrder程序启动："+chain_id.ToString());
            try
            {
                // 连接到以太坊区块链网络
                var web3 = new Web3(nodeHttps);
                // 读取JSON文件内容
                string jsonFilePath = "Ebay.json"; // 替换为正确的JSON文件路径

                string jsonString = File.ReadAllText(jsonFilePath);

                // 解析JSON
                JObject jsonObject = JObject.Parse(jsonString);

                // 获取abi节点的值
                string abi = jsonObject["abi"]?.ToString();

                var contract = new Contract(new EthApiService(web3.Client), abi, contractAddress);
                var function = contract.GetFunction("orders");

                var addOrder = Event<EbayAddOrderEventDTO>.GetEventABI().CreateFilterInput();
                var subscription = new EthLogsObservableSubscription(client);

                subscription.GetSubscriptionDataResponsesAsObservable().Subscribe(async log =>
                {
                 
                    // decode the log into a typed event log
                    var decoded = Event<EbayAddOrderEventDTO>.DecodeEvent(log);
                    if (decoded != null && log.Address.Equals(contractAddress, StringComparison.OrdinalIgnoreCase))
                    {
                        if (!_redisDb.LockTake(log.BlockHash, 1, TimeSpan.FromSeconds(10)))
                        {
                            return;
                        }
                        Console.WriteLine("EbayAddOrder监听到了！");
                        using (var scope = _serviceProvider.CreateScope())
                        {
                            var _masterDbContext = scope.ServiceProvider.GetRequiredService<MySqlMasterDbContext>();
                         
                            // 调用智能合约函数并获取返回结果
                            var orderResult = await function.CallDeserializingToObjectAsync<EbayOrderDTO>((int)decoded.Event.OrderId);
                            var chainToken = _masterDbContext.chain_tokens.Where(a => a.token_address.Equals(orderResult.Token) && a.chain_id == chain_id).FirstOrDefault();
                            var decimals_num = new BigDecimal(Math.Pow(10, chainToken.decimals));
                            var seller_pledge = (double)(new BigDecimal(orderResult.SellerPledge) / decimals_num);
                            var price = (double)(new BigDecimal(orderResult.Price) / decimals_num);
                            var amount = (double)orderResult.Amount;
                            var seller_ratio = (decimal)(seller_pledge / (price * amount));
                            var order = new orders() { amount = amount, buyer = orderResult.Buyer, buyer_contact = null, buyer_ex = (double)(new BigDecimal(orderResult.BuyerEx) / decimals_num), buyer_pledge = (double)(new BigDecimal(orderResult.BuyerPledge) / decimals_num), chain_id = chain_id, contract = contractAddress, create_time = DateTime.Now, creator = "system", description = orderResult.Description, img = orderResult.Img, name = orderResult.Name, seller = orderResult.Seller, order_id = (int)decoded.Event.OrderId, price = price, seller_contact = null, seller_pledge = seller_pledge, status = orderResult.Status, token = orderResult.Token, updater = null, update_time = DateTime.Now, weight = 10000, seller_ratio = seller_ratio,way=PostWayEnum.卖家发布 };
                            _masterDbContext.orders.Add(order);
                            _masterDbContext.SaveChanges();
                            _ = _sendMessage.SendMessageEbay((int)decoded.Event.OrderId, chain_id, contractAddress);
                        }
                    }

                });

                await client.StartAsync();

                await subscription.SubscribeAsync(addOrder);

            }
            catch (Exception ex)
            {
                Console.WriteLine($"EbayAddOrder:{ex}" + chain_id.ToString());
            }
        }
    }
}

