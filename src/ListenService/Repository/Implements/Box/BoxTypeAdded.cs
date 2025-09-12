using System.Net.WebSockets;
using System.Reactive.Linq;
using CommonLibrary.Common.Common;
using CommonLibrary.DbContext;
using CommonLibrary.Model.DataEntityModel;
using ListenService.Model;
using ListenService.Repository.Interfaces;
using Nethereum.Contracts;
using Nethereum.RPC.Reactive.Eth.Subscriptions;
using Newtonsoft.Json.Linq;
using StackExchange.Redis;

namespace ListenService.Repository.Implements
{
    public class BoxTypeAdded : IBoxTypeAdded
    {
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;
        private readonly IDatabase _redisDb;
        private readonly ClientManage _clientManage;
        private volatile bool _isRunning = false;
        private EthLogsObservableSubscription _subscription;

        public BoxTypeAdded(
            IConfiguration configuration,
            IServiceProvider serviceProvider,
            IDatabase redisDb,
            ClientManage clientManage
        )
        {
            _configuration = configuration;
            _serviceProvider = serviceProvider;
            _redisDb = redisDb;
            _clientManage = clientManage;
        }

        public async Task StartAsync(string nodeUrl, string contractAddress, ChainEnum chain_id)
        {
            if (_isRunning)
            {
                Console.WriteLine(
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                        + "BoxTypeAdded已经在运行中，跳过启动："
                        + chain_id
                );
                return;
            }

            _isRunning = true;
            Console.WriteLine(
                DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "BoxTypeAdded程序启动：" + chain_id
            );

            try
            {
                // 等待WebSocket连接就绪
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

                // 读取JSON文件内容
                string jsonFilePath = "DeMarketBox.json";
                string jsonString = System.IO.File.ReadAllText(jsonFilePath);
                JObject jsonObject = JObject.Parse(jsonString);
                string abi = jsonObject["abi"]?.ToString();

                var cardTypeAdded = Event<BoxTypeAddedEventDTO>.GetEventABI().CreateFilterInput();
                cardTypeAdded.Address = new string[] { contractAddress };
                _subscription = new EthLogsObservableSubscription(_clientManage.GetClient());

                _subscription
                    .GetSubscriptionDataResponsesAsObservable()
                    .Subscribe(
                        async log =>
                        {
                            await HandleLogAsync(log, contractAddress, chain_id);
                        },
                        async (ex) =>
                        {
                            Console.WriteLine(
                                DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                                    + $"BoxTypeAdded订阅异常:{ex}"
                            );
                            _isRunning = false;
                            await Task.Delay(2000);
                            await StartAsync(nodeUrl, contractAddress, chain_id);
                        }
                    );

                await _subscription.SubscribeAsync(cardTypeAdded);
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                        + $"BoxTypeAdded启动异常:{ex} - Chain ID: {chain_id}"
                );
                _isRunning = false;
                await CleanupResources();
                await Task.Delay(2000);
                await StartAsync(nodeUrl, contractAddress, chain_id);
            }
        }

        private async Task HandleLogAsync(
            Nethereum.RPC.Eth.DTOs.FilterLog log,
            string contractAddress,
            ChainEnum chain_id
        )
        {
            if (!_redisDb.LockTake(log.TransactionHash, 1, TimeSpan.FromSeconds(10)))
            {
                return;
            }

            // decode the log into a typed event log
            var decoded = Event<BoxTypeAddedEventDTO>.DecodeEvent(log);

            using (var scope = _serviceProvider.CreateScope())
            {
                var _masterDbContext =
                    scope.ServiceProvider.GetRequiredService<MySqlMasterDbContext>();
                Console.WriteLine(
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "BoxTypeAdded监听到了！"
                );
                var chainToken = _masterDbContext
                    .chain_tokens.Where(a =>
                        a.token_address.Equals(decoded.Event.TokenAddress) && a.chain_id == chain_id
                    )
                    .FirstOrDefault();
                var decimals_num = (double)Math.Pow(10, chainToken.decimals);
                var cardType = new card_type()
                {
                    type = decoded.Event.BoxType,
                    max_prize = (double)decoded.Event.MaxPrize / decimals_num,
                    max_prize_probability = (int)decoded.Event.MaxPrizeProbability,
                    name = decoded.Event.BoxName,
                    price = (double)decoded.Event.Price / decimals_num,
                    token = decoded.Event.TokenAddress,
                    winning_probability = (int)decoded.Event.WinningProbability,
                    chain_id = chain_id,
                    state = 1,
                    create_time = DateTime.Now,
                };
                _masterDbContext.card_type.Add(cardType);
                _masterDbContext.SaveChanges();
                Console.WriteLine(
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                        + "Contract address: "
                        + log.Address
                        + " Log Transfer from:"
                        + decoded.Event.BoxName
                );
            }
        }

        private async Task CleanupResources()
        {
            try
            {
                if (_subscription != null)
                {
                    _clientManage.GetClient().RemoveSubscription(_subscription.SubscriptionId);
                    _subscription = null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + $"BoxTypeAdded清理资源异常:{ex}"
                );
            }
        }
    }
}
