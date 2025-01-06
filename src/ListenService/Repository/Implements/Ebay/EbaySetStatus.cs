using CommonLibrary.Common.Common;
using CommonLibrary.DbContext;
using ListenService;
using ListenService.Model;
using ListenService.Repository.Interfaces;
using Nethereum.Contracts;
using Nethereum.RPC;
using Nethereum.RPC.Reactive.Eth.Subscriptions;
using Nethereum.Util;
using Nethereum.Web3;
using Newtonsoft.Json.Linq;
using StackExchange.Redis;
using System.Net.WebSockets;

public class EbaySetStatus : IEbaySetStatus
{
    private readonly IConfiguration _configuration;
    private readonly IServiceProvider _serviceProvider;
    private readonly ISendMessage _sendMessage;
    private readonly IDatabase _redisDb;
    private string _abi;  // 将 abi 提升为类成员
    private Web3 _web3;   // 将 Web3 实例提升为类成员
    private Contract _contract; // 将 Contract 实例提升为类成员
    private readonly ClientManage _clientManage;

    public EbaySetStatus(IConfiguration configuration, IServiceProvider serviceProvider, ISendMessage sendMessage, IDatabase redisDb, ClientManage clientManage)
    {
        _configuration = configuration;
        _serviceProvider = serviceProvider;
        _sendMessage = sendMessage;
        _redisDb = redisDb;
        _clientManage = clientManage;
    }

    public async Task StartAsync(string nodeWss, string nodeHttps, string contractAddress, ChainEnum chainId)
    {
        Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "EbaySetStatus程序启动：" + chainId);

        try
        {
            while (true)
            {
                if (_clientManage.GetClient().WebSocketState == WebSocketState.Open)
                {
                    break;
                }
                else
                {
                    await Task.Delay(500).ConfigureAwait(false);
                }
            }
            // 读取 JSON 文件内容并解析 ABI
            string jsonFilePath = "Ebay.json"; // 替换为正确的 JSON 文件路径
            string jsonString = File.ReadAllText(jsonFilePath);
            JObject jsonObject = JObject.Parse(jsonString);
            _abi = jsonObject["abi"]?.ToString();

            // 初始化 Web3 和 Contract，避免重复创建
            _web3 = new Web3(nodeHttps);
            _contract = new Contract(new EthApiService(_web3.Client), _abi, contractAddress);

            var addOrder = Event<EbaySetStatusEventDTO>.GetEventABI().CreateFilterInput();
            addOrder.Address = new string[] { contractAddress };
            var subscription = new EthLogsObservableSubscription(_clientManage.GetClient());

            subscription.GetSubscriptionDataResponsesAsObservable().Subscribe(async log =>
            {
                try
                {
                    await HandleLogAsync(log, contractAddress, chainId);
                }
                catch (Exception ex)
                {
                    _clientManage.GetClient().RemoveSubscription(subscription.SubscriptionId);
                    Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + $"EbaySetStatus1:{ex}");
                    await Task.Delay(2000);
                    await StartAsync(nodeWss, nodeHttps, contractAddress, chainId);
                }
            }, async (ex) =>
            {
                _clientManage.GetClient().RemoveSubscription(subscription.SubscriptionId);
                Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + $"EbaySetStatus2:{ex}");
                await Task.Delay(2000);
                await StartAsync(nodeWss, nodeHttps, contractAddress, chainId);
            });

            await subscription.SubscribeAsync(addOrder);
        }
        catch (Exception ex)
        {
            Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + $"EbaySetStatus3:{ex} - Chain ID: {chainId}");
            await Task.Delay(2000);
            await StartAsync(nodeWss, nodeHttps, contractAddress, chainId);
        }
    }

    private async Task HandleLogAsync(Nethereum.RPC.Eth.DTOs.FilterLog log, string contractAddress, ChainEnum chainId)
    {
        // 检查事件来源是否符合要求
        var decoded = Event<EbaySetStatusEventDTO>.DecodeEvent(log);

        if (!_redisDb.LockTake(log.TransactionHash + "EbaySet", 1, TimeSpan.FromSeconds(10)))
        {
            return;
        }
        Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "EbaySetStatus监听到了！" + chainId);

        using (var scope = _serviceProvider.CreateScope())
        {
            var _masterDbContext = scope.ServiceProvider.GetRequiredService<MySqlMasterDbContext>();
            var function = _contract.GetFunction("orders");
            //await Task.Delay(2000);
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