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

namespace ListenService.Repository.Implements
{
    public class EbaySetStatus : IEbaySetStatus
    {
        private readonly IConfiguration _configuration;
        private readonly MySqlMasterDbContext _masterDbContext;
        private readonly ISendMessage _sendMessage;
        public EbaySetStatus(IConfiguration configuration, MySqlMasterDbContext mySqlMasterDbContext, ISendMessage sendMessage)
        {
            _configuration = configuration;
            _masterDbContext = mySqlMasterDbContext;
            _sendMessage = sendMessage;

        }
        public async Task StartAsync(string nodeWss, string nodeHttps, string contractAddress, ChainEnum chain_id)
        {
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

                StreamingWebSocketClient.ForceCompleteReadTotalMilliseconds = Timeout.Infinite;
                //StreamingWebSocketClient.ConnectionTimeout = Timeout.InfiniteTimeSpan;
                var client = new StreamingWebSocketClient(nodeWss);

                var addOrder = Event<SetStatusEventDTO>.GetEventABI().CreateFilterInput();
                var subscription = new EthLogsObservableSubscription(client);

                Action<Exception> onErrorAction = async (ex) =>
                {
                    // 处理异常情况 ex
                    Console.WriteLine($"Error EbaySetStatus: {ex}");
                    await StartAsync(nodeWss, nodeHttps, contractAddress, chain_id);
                };

                subscription.GetSubscriptionDataResponsesAsObservable().Subscribe(async log =>
                {

                    var decoded = Event<SetStatusEventDTO>.DecodeEvent(log);
                    if (decoded != null && log.Address.Equals(contractAddress, StringComparison.OrdinalIgnoreCase))
                    {
                        Console.WriteLine("EbaySetStatus监听到了！");
                        // 调用智能合约函数并获取返回结果
                        var orderResult = await function.CallDeserializingToObjectAsync<EbayOrderDTO>((int)decoded.Event.OrderId);
                        var chainToken = _masterDbContext.chain_tokens.Where(a => a.token_address.Equals(orderResult.Token) && a.chain_id == chain_id).FirstOrDefault();
                        var decimals_num = (double)Math.Pow(10, chainToken.decimals);
                        var order = _masterDbContext.orders.Where(a => a.order_id == (int)decoded.Event.OrderId && a.chain_id == chain_id && a.contract.Equals(contractAddress)).FirstOrDefault();

                        if (orderResult.Status == OrderStatus.Ordered)
                        {
                            order.create_time = DateTime.Now;
                        }
                        order.status = orderResult.Status;
                        order.buyer_ex = (double)orderResult.BuyerEx / decimals_num;
                        order.update_time = DateTime.Now;
                        order.buyer = orderResult.Buyer;
                        order.buyer_pledge = (double)orderResult.BuyerPledge / decimals_num;
                        order.seller_pledge = (double)orderResult.SellerPledge / decimals_num;
                        order.amount = (double)orderResult.Amount;
                        order.price = (double)orderResult.Price / decimals_num;

                        _masterDbContext.SaveChanges();
                        _ = _sendMessage.SendMessageEbay((int)decoded.Event.OrderId, chain_id, contractAddress);
                    }
                    else
                    {
                        Console.WriteLine("EbaySetStatus:Found not standard log");
                    }

                }, onErrorAction);

                await client.StartAsync();

                await subscription.SubscribeAsync(addOrder);

            }
            catch (Exception ex)
            {
                await StartAsync(nodeWss, nodeHttps, contractAddress, chain_id);
                Console.WriteLine($"EbaySetStatus:{ex}");
                Console.WriteLine("EbaySetStatus重启了EX");
            }
        }

    }
}

