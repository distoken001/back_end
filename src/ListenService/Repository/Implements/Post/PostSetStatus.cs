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
using Nethereum.Util;

namespace ListenService.Repository.Implements
{
    public class PostSetStatus : IPostSetStatus
    {
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;
        private readonly ISendMessage _sendMessage;
        public PostSetStatus(IConfiguration configuration, IServiceProvider serviceProvider, ISendMessage sendMessage)
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
            Console.WriteLine("PostSetStatus程序启动：" + chain_id.ToString());
            try
            {
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


                var postSetStatus = Event<PostSetStatusEventDTO>.GetEventABI().CreateFilterInput();
                var subscription = new EthLogsObservableSubscription(client);

                Action<Exception> onErrorAction = async (ex) =>
                {
                    // 处理异常情况 ex
                    Console.WriteLine($"Error PostSetStatus: {ex}");
                    client.Dispose();
                    await StartAsync(nodeWss, nodeHttps, contractAddress, chain_id);
                };

                subscription.GetSubscriptionDataResponsesAsObservable().Subscribe(async log =>
                {
                    Console.WriteLine("PostSetStatus监听到了！");
                    var decoded = Event<PostSetStatusEventDTO>.DecodeEvent(log);
                    if (decoded != null && log.Address.Equals(contractAddress, StringComparison.OrdinalIgnoreCase))
                    {
                        using (var scope = _serviceProvider.CreateScope())
                        {
                            var _masterDbContext = scope.ServiceProvider.GetRequiredService<MySqlMasterDbContext>();
                            // 调用智能合约函数并获取返回结果
                            var postResult = await function.CallDeserializingToObjectAsync<PostOrderDTO>((int)decoded.Event.OrderId);
                            var chainToken = _masterDbContext.chain_tokens.Where(a => a.token_address.Equals(postResult.Token) && a.chain_id == chain_id).FirstOrDefault();
                            var decimals_num = new BigDecimal(Math.Pow(10, chainToken.decimals));
                            var post = _masterDbContext.orders.Where(a => a.order_id == (int)decoded.Event.OrderId && a.chain_id == chain_id && a.contract.Equals(contractAddress)).FirstOrDefault();

                            if (postResult.Status == OrderStatus.Ordered)
                            {
                                post.create_time = DateTime.Now;
                            }
                            post.status = postResult.Status;
                            post.buyer_ex = (double)(new BigDecimal(postResult.BuyerEx) / decimals_num);
                            post.update_time = DateTime.Now;
                            post.buyer = postResult.Buyer;
                            post.buyer_pledge = (double)(new BigDecimal(postResult.BuyerPledge) / decimals_num);
                            post.seller_pledge = (double)(new BigDecimal(postResult.SellerPledge) / decimals_num);
                            post.amount = (double)postResult.Amount;
                            post.price = (double)(new BigDecimal(postResult.Price) / decimals_num);
                            post.seller = postResult.Seller;

                            _masterDbContext.SaveChanges();
                            _ = _sendMessage.SendMessagePost((int)decoded.Event.OrderId, chain_id, contractAddress);
                        }
                    }
                    else
                    {
                        Console.WriteLine("PostSetStatus:Found not standard log");
                    }

                }, onErrorAction);

                await client.StartAsync();

                await subscription.SubscribeAsync(postSetStatus);

            }
            catch (Exception ex)
            {
                client.Dispose();
                await StartAsync(nodeWss, nodeHttps, contractAddress, chain_id);
                Console.WriteLine($"PostSetStatus:{ex}");
                Console.WriteLine("PostSetStatus重启了EX");
            }
        }

    }
}

