using System.Net.WebSockets;
using CommonLibrary.Common.Common;
using CommonLibrary.DbContext;
using CommonLibrary.Model.DataEntityModel;
using ListenService;
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

public class EbayAddOrder : IEbayAddOrder
{
    private readonly IConfiguration _configuration;
    private readonly IServiceProvider _serviceProvider;
    private readonly ISendMessage _sendMessage;
    private readonly IDatabase _redisDb;
    private string _abi;  // 将 abi 提升为类成员
    private Web3 _web3;   // 将 Web3 实例提升为类成员
    private Contract _contract; // 将 Contract 实例提升为类成员
    private readonly ClientManage _clientManage;

    public EbayAddOrder(IConfiguration configuration, IServiceProvider serviceProvider, ISendMessage sendMessage, IDatabase redisDb, ClientManage clientManage)
    {
        _configuration = configuration;
        _serviceProvider = serviceProvider;
        _sendMessage = sendMessage;
        _redisDb = redisDb;
        _clientManage = clientManage;
    }

    public async Task StartAsync(string nodeWss, string nodeHttps, string contractAddress, ChainEnum chainId)
    {
        Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "EbayAddOrder程序启动：" + chainId);

        try
        {
           while(true)
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
            string jsonFilePath = "Ebay.json";
            string jsonString = File.ReadAllText(jsonFilePath);
            JObject jsonObject = JObject.Parse(jsonString);
            _abi = jsonObject["abi"]?.ToString();

            // 初始化 Web3 和 Contract，避免重复创建
            _web3 = new Web3(nodeHttps);
            _contract = new Contract(new EthApiService(_web3.Client), _abi, contractAddress);

            var addOrder = Event<EbayAddOrderEventDTO>.GetEventABI().CreateFilterInput();
            addOrder.Address = new string[] { contractAddress };
            var subscription = new EthLogsObservableSubscription(_clientManage.GetClient());

            subscription.GetSubscriptionDataResponsesAsObservable().Subscribe(async log =>
            {
                try
                {
                    Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "EbayAddOrder监听到了！");
                    await HandleLogAsync(log, contractAddress, chainId);
                }
                catch(Exception ex)
                {
                    _clientManage.GetClient().RemoveSubscription(subscription.SubscriptionId);
                    Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + $"EbayAddOrder1:{ex}");
                    await Task.Delay(2000);
                    await StartAsync(nodeWss, nodeHttps, contractAddress, chainId);
                }
            }, async (ex) => {
                _clientManage.GetClient().RemoveSubscription(subscription.SubscriptionId);
                Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + $"EbayAddOrder2:{ex}");
                await Task.Delay(2000);
                await StartAsync(nodeWss, nodeHttps, contractAddress, chainId);
            });

            await subscription.SubscribeAsync(addOrder);
        }
        catch (Exception ex)
        {
            Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + $"EbayAddOrder3:{ex} - Chain ID: {chainId}");
            await Task.Delay(2000);
            await StartAsync(nodeWss, nodeHttps,contractAddress, chainId);
        }
    }

    private async Task HandleLogAsync(Nethereum.RPC.Eth.DTOs.FilterLog log, string contractAddress, ChainEnum chainId)
    {
      
       
            if (!_redisDb.LockTake(log.TransactionHash, 1, TimeSpan.FromSeconds(10)))
            {
                return;
            }

           
            using (var scope = _serviceProvider.CreateScope())
            {
                var _masterDbContext = scope.ServiceProvider.GetRequiredService<MySqlMasterDbContext>();

                // 获取 orders 函数并调用智能合约
                var function = _contract.GetFunction("orders");
                var decoded = Event<EbayAddOrderEventDTO>.DecodeEvent(log);

                if (decoded != null)
                {
                    var orderResult = await function.CallDeserializingToObjectAsync<EbayOrderDTO>((int)decoded.Event.OrderId);
                    var chainToken = _masterDbContext.chain_tokens
                        .FirstOrDefault(a => a.token_address.Equals(orderResult.Token) && a.chain_id == chainId);

                    if (chainToken != null)
                    {
                        // 处理订单数据
                        var decimalsNum = new BigDecimal(Math.Pow(10, chainToken.decimals));
                        var sellerPledge = (double)(new BigDecimal(orderResult.SellerPledge) / decimalsNum);
                        var price = (double)(new BigDecimal(orderResult.Price) / decimalsNum);
                        var amount = (double)orderResult.Amount;
                        var sellerRatio = (decimal)(sellerPledge / (price * amount));

                        var order = new orders
                        {
                            amount = amount,
                            buyer = orderResult.Buyer,
                            buyer_contact = null,
                            buyer_ex = (double)(new BigDecimal(orderResult.BuyerEx) / decimalsNum),
                            buyer_pledge = (double)(new BigDecimal(orderResult.BuyerPledge) / decimalsNum),
                            chain_id = chainId,
                            contract = contractAddress,
                            create_time = DateTime.Now,
                            creator = "system",
                            description = orderResult.Description,
                            img = orderResult.Img,
                            name = orderResult.Name,
                            seller = orderResult.Seller,
                            order_id = (int)decoded.Event.OrderId,
                            price = price,
                            seller_contact = null,
                            seller_pledge = sellerPledge,
                            status = orderResult.Status,
                            token = orderResult.Token,
                            updater = null,
                            update_time = DateTime.Now,
                            weight = 10000,
                            seller_ratio = sellerRatio,
                            way = PostWayEnum.卖家发布
                        };

                        _masterDbContext.orders.Add(order);
                        await _masterDbContext.SaveChangesAsync();

                        _ = _sendMessage.SendMessageEbay((int)decoded.Event.OrderId, chainId, contractAddress);
                    }
                }
            }
        }
}
