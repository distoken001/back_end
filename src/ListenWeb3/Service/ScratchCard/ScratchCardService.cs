using System;
using System.Threading.Tasks;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using Nethereum.Contracts;
using Nethereum.RPC.Eth.DTOs;
using CommonLibrary.DbContext;
using Newtonsoft.Json.Linq;
using Nethereum.JsonRpc.WebSocketClient;
using Nethereum.JsonRpc.WebSocketStreamingClient;
using Nethereum.RPC.Reactive.Eth.Subscriptions;
using Newtonsoft.Json;
using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Nethereum.ABI.Model;
using ListenWeb3.Model;
using Nethereum.JsonRpc.Client;
using CommonLibrary.Model.DataEntityModel;
using CommonLibrary.Common.Common;

namespace ListenWeb3.Service.ScratchCard
{
    public class ScratchCardService : IHostedService
    {
        private readonly IConfiguration _configuration;
        private readonly MySqlMasterDbContext _masterDbContext;
        public ScratchCardService(IConfiguration configuration, MySqlMasterDbContext mySqlMasterDbContext)
        {
            _configuration = configuration;
            _masterDbContext = mySqlMasterDbContext;
        }


        public async Task StartAsync(CancellationToken cancellationToken)
        {

            try
            {
                // Infura 提供的以太坊节点 WebSocket 地址
                string nodeUrl = _configuration["OP:WSS_URL"];

                // 你的以太坊智能合约地址
                string contractAddress = _configuration["OP:Contract_ScratchCard"];

                // 读取JSON文件内容
                string jsonFilePath = "ScratchCard.json"; // 替换为正确的JSON文件路径

                string jsonString = System.IO.File.ReadAllText(jsonFilePath);

                // 解析JSON
                JObject jsonObject = JObject.Parse(jsonString);

                // 获取abi节点的值
                string abi = jsonObject["abi"]?.ToString();

                var client = new StreamingWebSocketClient(nodeUrl);

                var cardTypeAdded = Event<CardTypeAddedEventDTO>.GetEventABI().CreateFilterInput();

                var subscription = new EthLogsObservableSubscription(client);
                // attach a handler for Transfer event logs
                subscription.GetSubscriptionDataResponsesAsObservable().Subscribe(log =>
                {
                    try
                    {
                        // decode the log into a typed event log
                        var decoded = Event<CardTypeAddedEventDTO>.DecodeEvent(log);
                        if (decoded != null)
                        {
                            ChainEnum chain_id = ChainEnum.OptimisticGoerli;
                            if (_configuration["Env"] == "prod")
                            {
                                chain_id = ChainEnum.Optimism;
                            }

                            var cardType = new card_type() { type = decoded.Event.CardType, max_prize = (double)decoded.Event.MaxPrize, max_prize_probability = (int)decoded.Event.MaxPrizeProbability, name = decoded.Event.CardName, price = (double)decoded.Event.Price, token = decoded.Event.TokenAddress, winning_probability = (int)decoded.Event.WinningProbability, chain_id = chain_id };
                            _masterDbContext.card_type.Add(cardType);
                            _masterDbContext.SaveChanges();
                            Console.WriteLine("Contract address: " + log.Address + " Log Transfer from:" + decoded.Event.CardName);
                        }
                        else
                        {

                            Console.WriteLine("Found not standard CardTypeAddedEvent log");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Log Address: " + log.Address + " is not a standard transfer log:", ex.Message);
                    }
                });
                // open the web socket connection
                await client.StartAsync();

                // begin receiving subscription data
                // data will be received on a background thread
                await subscription.SubscribeAsync(cardTypeAdded);

                //// run for a while
                //await Task.Delay(TimeSpan.FromMinutes(1));

                //// unsubscribe
                //await subscription.UnsubscribeAsync();

                //// allow time to unsubscribe
                //await Task.Delay(TimeSpan.FromSeconds(5));

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
        public Task StopAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
