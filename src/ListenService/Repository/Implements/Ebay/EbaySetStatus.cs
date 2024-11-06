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
using StackExchange.Redis;

public class EbaySetStatus : IEbaySetStatus
{
    private readonly IConfiguration _configuration;
    private readonly IServiceProvider _serviceProvider;
    private readonly ISendMessage _sendMessage;
    private readonly IDatabase _redisDb;
    private string _abi;  // 将 abi 提升为类成员
    private Web3 _web3;   // 将 Web3 实例提升为类成员
    private Contract _contract; // 将 Contract 实例提升为类成员

    public EbaySetStatus(IConfiguration configuration, IServiceProvider serviceProvider, ISendMessage sendMessage, IDatabase redisDb)
    {
        _configuration = configuration;
        _serviceProvider = serviceProvider;
        _sendMessage = sendMessage;
        _redisDb = redisDb;
    }

    public async Task StartAsync(string nodeWss, string nodeHttps, string contractAddress, ChainEnum chainId)
    {
        StreamingWebSocketClient.ForceCompleteReadTotalMilliseconds = Timeout.Infinite;
        var client = new StreamingWebSocketClient(nodeWss);
        Console.WriteLine("EbaySetStatus程序启动：" + chainId);

        try
        {
            // 读取 JSON 文件内容并解析 ABI
            string jsonFilePath = "Ebay.json"; // 替换为正确的 JSON 文件路径
            string jsonString = File.ReadAllText(jsonFilePath);
            JObject jsonObject = JObject.Parse(jsonString);
            _abi = jsonObject["abi"]?.ToString();

            // 初始化 Web3 和 Contract，避免重复创建
            _web3 = new Web3(nodeHttps);
            _contract = new Contract(new EthApiService(_web3.Client), _abi, contractAddress);

            var addOrder = Event<EbaySetStatusEventDTO>.GetEventABI().CreateFilterInput();
            var subscription = new EthLogsObservableSubscription(client);

            subscription.GetSubscriptionDataResponsesAsObservable().Subscribe(async log =>
            {
                await HandleLogAsync(log, contractAddress, chainId);
            });

            await client.StartAsync();
            await subscription.SubscribeAsync(addOrder);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"EbaySetStatus:{ex} - Chain ID: {chainId}");
        }
    }

    private async Task HandleLogAsync(Nethereum.RPC.Eth.DTOs.FilterLog log, string contractAddress, ChainEnum chainId)
    {
        // 检查事件来源是否符合要求
        var decoded = Event<EbaySetStatusEventDTO>.DecodeEvent(log);
        if (decoded != null && log.Address.Equals(contractAddress, StringComparison.OrdinalIgnoreCase))
        {
            if (!_redisDb.LockTake(log.TransactionHash, 1, TimeSpan.FromSeconds(10)))
            {
                return;
            }

            Console.WriteLine("EbaySetStatus监听到事件：" + chainId);

            using (var scope = _serviceProvider.CreateScope())
            {
                var _masterDbContext = scope.ServiceProvider.GetRequiredService<MySqlMasterDbContext>();
                var function = _contract.GetFunction("orders");

                // 获取智能合约中的订单信息
                var orderResult = await function.CallDeserializingToObjectAsync<EbayOrderDTO>((int)decoded.Event.OrderId);
                var chainToken = _masterDbContext.chain_tokens
                    .FirstOrDefault(a => a.token_address.Equals(orderResult.Token) && a.chain_id == chainId);

                if (chainToken != null)
                {
                    var decimalsNum = new BigDecimal(Math.Pow(10, chainToken.decimals));
                    var order = _masterDbContext.orders
                        .FirstOrDefault(a => a.order_id == (int)decoded.Event.OrderId && a.chain_id == chainId && a.contract.Equals(contractAddress));

                    if (order != null)
                    {
                        // 更新订单信息
                        if (orderResult.Status == OrderStatus.Ordered)
                        {
                            order.create_time = DateTime.Now;
                        }

                        order.status = orderResult.Status;
                        order.buyer_ex = (double)(new BigDecimal(orderResult.BuyerEx) / decimalsNum);
                        order.update_time = DateTime.Now;
                        order.buyer = orderResult.Buyer;
                        order.buyer_pledge = (double)(new BigDecimal(orderResult.BuyerPledge) / decimalsNum);
                        order.seller_pledge = (double)(new BigDecimal(orderResult.SellerPledge) / decimalsNum);
                        order.amount = (double)orderResult.Amount;
                        order.price = (double)(new BigDecimal(orderResult.Price) / decimalsNum);
                        order.seller_ratio = (decimal)(order.seller_pledge / (order.price * order.amount));

                        _masterDbContext.SaveChanges();

                        // 发送消息
                        _ = _sendMessage.SendMessageEbay((int)decoded.Event.OrderId, chainId, contractAddress);
                    }
                }
            }
        }
    }
}
