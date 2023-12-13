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

namespace ListenService.Repository.Implements
{
    public class EbayAddOrder : IEbayAddOrder
    {
        private readonly IConfiguration _configuration;
        private readonly MySqlMasterDbContext _masterDbContext;
        private readonly ISendMessage _sendMessage;
        public EbayAddOrder(IConfiguration configuration, MySqlMasterDbContext mySqlMasterDbContext,ISendMessage sendMessage)
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

                var addOrder = Event<AddOrderEventDTO>.GetEventABI().CreateFilterInput();
                var subscription = new EthLogsObservableSubscription(client);

                Action<Exception> onErrorAction = async (ex) =>
                {
                    // 处理异常情况 ex
                    Console.WriteLine($"Error EbayAddOrder: {ex}");
                    await StartAsync(nodeWss, nodeHttps, contractAddress, chain_id);
                };
                // attach a handler for Transfer event logs
                subscription.GetSubscriptionDataResponsesAsObservable().Subscribe(async log =>
                {
                    // decode the log into a typed event log
                    var decoded = Event<AddOrderEventDTO>.DecodeEvent(log);
                    if (decoded != null && log.Address.Equals(contractAddress, StringComparison.OrdinalIgnoreCase))
                    {
                        Console.WriteLine("EbayAddOrder监听到了！");
                        // 调用智能合约函数并获取返回结果
                        var orderResult = await function.CallDeserializingToObjectAsync<EbayOrderDTO>((int)decoded.Event.OrderId);
                        var chainToken = _masterDbContext.chain_tokens.Where(a => a.token_address.Equals(orderResult.Token) && a.chain_id == chain_id).FirstOrDefault();
                        var decimals_num = (double)Math.Pow(10, chainToken.decimals);
                        var order = new orders() { amount = (double)orderResult.Amount, buyer = orderResult.Buyer, buyer_contact = null, buyer_ex = (double)orderResult.BuyerEx / decimals_num, buyer_pledge = (double)orderResult.BuyerPledge, chain_id = chain_id, contract = contractAddress, create_time = DateTime.Now, creator = "system", description = orderResult.Description, img = orderResult.Img, name = orderResult.Name, seller = orderResult.Seller, order_id = (int)decoded.Event.OrderId, price = (double)orderResult.Price / decimals_num, seller_contact = null, seller_pledge = (double)orderResult.SellerPledge / decimals_num, status = orderResult.Status, token = orderResult.Token, updater = null, update_time = DateTime.Now, weight = 10000 };
                        _masterDbContext.orders.Add(order);
                        _masterDbContext.SaveChanges();
                        _ = _sendMessage.SendMessageEbay((int)decoded.Event.OrderId, chain_id, contractAddress);


                    }
                    else
                    {
                        Console.WriteLine("EbayAddOrder:Found not standard log");
                    }

                }, onErrorAction);

                await client.StartAsync();

                await subscription.SubscribeAsync(addOrder);

            }
            catch (Exception ex)
            {
                await StartAsync(nodeWss, nodeHttps, contractAddress, chain_id);
                Console.WriteLine($"EbayAddOrder:{ex}");
                Console.WriteLine("EbayAddOrder重启了EX");
            }
        }
    }
}

