using Nethereum.Contracts;
using CommonLibrary.DbContext;
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
using Newtonsoft.Json.Linq;
using Nethereum.Web3;
using Nethereum.RPC;
using Org.BouncyCastle.Asn1.X509;
using Microsoft.VisualBasic;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Numerics;
using Nethereum.Util;
using StackExchange.Redis;

namespace ListenService.Repository.Implements
{
    public class PostAddOrder : IPostAddOrder
    {
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;
        private readonly ISendMessage _sendMessage;
        private readonly IDatabase _redisDb;
        private StreamingWebSocketClient _client;
        public PostAddOrder(IConfiguration configuration, IServiceProvider serviceProvider, ISendMessage sendMessage,IDatabase redisDb,StreamingWebSocketClient client)
        {
            _configuration = configuration;
            _serviceProvider = serviceProvider;
            _sendMessage = sendMessage;
            _redisDb = redisDb;
            _client = client;
        }
        public async Task StartAsync(string nodeWss, string nodeHttps, string contractAddress, ChainEnum chain_id)
        {
          
            Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "PostAddOrder程序启动：" + chain_id.ToString());
            try
            {
                for (int i = 0; i < 10; i++)
                {
                    if (_client.WebSocketState == WebSocketState.Open)
                    {
                        break;
                    }
                    else
                    {
                        await Task.Delay(500).ConfigureAwait(false);
                    }
                }
                // 连接到以太坊区块链网络
                var web3 = new Web3(nodeHttps);
                // 读取JSON文件内容
                string jsonFilePath = "Post.json"; // 替换为正确的JSON文件路径

                string jsonString = File.ReadAllText(jsonFilePath);

                // 解析JSON
                JObject jsonObject = JObject.Parse(jsonString);

                // 获取abi节点的值
                string abi = jsonObject["abi"]?.ToString();

                var contract = new Contract(new EthApiService(web3.Client), abi, contractAddress);
                var function = contract.GetFunction("orders");

                var functionExtend = contract.GetFunction("extend");

                var addPost = Event<PostAddOrderEventDTO>.GetEventABI().CreateFilterInput();
                addPost.Address = new string[] { contractAddress };
                var subscription = new EthLogsObservableSubscription(_client);

                //Action<Exception> onErrorAction = async (ex) =>
                //{
                //    // 处理异常情况 ex
                //    Console.WriteLine($"Error PostAddOrder: {ex}");
                //    client.Dispose();
                //    await StartAsync(nodeWss, nodeHttps, contractAddress, chain_id);
                //};
                // attach a handler for Transfer event logs
                subscription.GetSubscriptionDataResponsesAsObservable().Subscribe(async log =>
                {
                    try { 
                    Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "PostAddOrder监听到了！" + chain_id.ToString());
                    if (!_redisDb.LockTake(log.TransactionHash, 1, TimeSpan.FromSeconds(10)))
                    {
                        return;
                    }
                    // decode the log into a typed event log
                    var decoded = Event<PostAddOrderEventDTO>.DecodeEvent(log);

                        using (var scope = _serviceProvider.CreateScope())
                        {
                            var _masterDbContext = scope.ServiceProvider.GetRequiredService<MySqlMasterDbContext>();

                            // 调用智能合约函数并获取返回结果
                            var postResult = await function.CallDeserializingToObjectAsync<PostOrderDTO>((int)decoded.Event.OrderId);
                            var extendResult = await functionExtend.CallDeserializingToObjectAsync<ExtendDTO>((int)decoded.Event.OrderId);
                            var chainToken = _masterDbContext.chain_tokens.Where(a => a.token_address.Equals(postResult.Token) && a.chain_id == chain_id).FirstOrDefault();
                            var decimals_num = new BigDecimal(Math.Pow(10, chainToken.decimals));
                            var post = new orders() { amount = (double)postResult.Amount, buyer = postResult.Buyer, buyer_contact = null, buyer_ex = (double)(new BigDecimal(postResult.BuyerEx) / decimals_num), buyer_pledge = (double)(new BigDecimal(postResult.BuyerPledge) / decimals_num), chain_id = chain_id, contract = contractAddress, create_time = DateTime.Now, creator = "system", description = postResult.Description, img = postResult.Img, name = postResult.Name, seller = postResult.Seller, order_id = (int)decoded.Event.OrderId, price = (double)(new BigDecimal(postResult.Price) / decimals_num), seller_contact = null, seller_pledge = (double)(new BigDecimal(postResult.SellerPledge) / decimals_num), status = postResult.Status, token = postResult.Token, updater = null, update_time = DateTime.Now, weight = 10000, seller_ratio = (decimal)(new BigDecimal(extendResult.SellerRatio) / new BigDecimal(10000)), way = PostWayEnum.买家发布 };
                            _masterDbContext.orders.Add(post);
                            _masterDbContext.SaveChanges();
                            _ = _sendMessage.SendMessagePost((int)decoded.Event.OrderId, chain_id, contractAddress);
                        }
                    }
                    catch(Exception ex)
                    {
                        _client.RemoveSubscription(subscription.SubscriptionId);
                        Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + $"PostAddOrder1:{ex}");
                        await Task.Delay(2000);
                        await StartAsync(nodeWss, nodeHttps, contractAddress, chain_id);
                    }
                   

                }, async (ex) => {
                    _client.RemoveSubscription(subscription.SubscriptionId);
                    Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + $"PostAddOrder2:{ex}");
                    await Task.Delay(2000);
                    await StartAsync(nodeWss, nodeHttps, contractAddress, chain_id);
                });

          

                await subscription.SubscribeAsync(addPost);

            }
            catch (Exception ex)
            {
                Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + $"PostAddOrder3:{ex} - Chain ID: {chain_id}");
                await Task.Delay(2000);
                await StartAsync(nodeWss, nodeHttps, contractAddress, chain_id);
            }
        }
    }
}

