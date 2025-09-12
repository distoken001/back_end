using System.Net.WebSockets;
using System.Reactive.Linq;
using CommonLibrary.Common.Common;
using CommonLibrary.DbContext;
using ListenService.Model;
using ListenService.Repository.Interfaces;
using Nethereum.Contracts;
using Nethereum.RPC.Reactive.Eth.Subscriptions;
using StackExchange.Redis;

namespace ListenService.Repository.Implements
{
    public class BoxTypeRemoved : IBoxTypeRemoved
    {
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;
        private readonly IDatabase _redisDb;
        private readonly ClientManage _clientManage;
        private volatile bool _isRunning = false;
        private EthLogsObservableSubscription _subscription;

        public BoxTypeRemoved(
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
                        + "BoxTypeRemoved已经在运行中，跳过启动："
                        + chain_id
                );
                return;
            }

            _isRunning = true;
            Console.WriteLine(
                DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "BoxTypeRemoved程序启动：" + chain_id
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

                var cardTypeRemoved = Event<BoxTypeRemovedEventDTO>
                    .GetEventABI()
                    .CreateFilterInput();
                cardTypeRemoved.Address = new string[] { contractAddress };
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
                                    + $"BoxTypeRemoved订阅异常:{ex}"
                            );
                            _isRunning = false;
                            await Task.Delay(2000);
                            await StartAsync(nodeUrl, contractAddress, chain_id);
                        }
                    );

                await _subscription.SubscribeAsync(cardTypeRemoved);
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                        + $"BoxTypeRemoved启动异常:{ex} - Chain ID: {chain_id}"
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

            Console.WriteLine(
                DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "BoxTypeRemoved监听到了！"
            );
            // decode the log into a typed event log
            var decoded = Event<BoxTypeRemovedEventDTO>.DecodeEvent(log);

            using (var scope = _serviceProvider.CreateScope())
            {
                var _masterDbContext =
                    scope.ServiceProvider.GetRequiredService<MySqlMasterDbContext>();
                var card = _masterDbContext
                    .card_type.Where(a =>
                        a.type == decoded.Event.BoxType && a.chain_id == chain_id && a.state == 1
                    )
                    .FirstOrDefault();
                card.state = 0;
                _masterDbContext.SaveChanges();
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
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                        + $"BoxTypeRemoved清理资源异常:{ex}"
                );
            }
        }
    }
}
