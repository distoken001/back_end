using System.Net.WebSockets;
using System.Reactive.Linq;
using CommonLibrary.Common.Common;
using CommonLibrary.DbContext;
using CommonLibrary.Model.DataEntityModel;
using ListenService.Model;
using ListenService.Repository.Interfaces;
using Nethereum.Contracts;
using Nethereum.RPC.Reactive.Eth.Subscriptions;
using StackExchange.Redis;

namespace ListenService.Repository.Implements
{
    public class PrizeClaimed : IPrizeClaimed
    {
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;
        private readonly IDatabase _redisDb;
        private readonly ClientManage _clientManage;
        private volatile bool _isRunning = false;
        private EthLogsObservableSubscription _subscription;

        public PrizeClaimed(
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
                        + "PrizeClaimed已经在运行中，跳过启动："
                        + chain_id
                );
                return;
            }

            _isRunning = true;
            Console.WriteLine(
                DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "PrizeClaimed程序启动：" + chain_id
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

                var prizeClaimed = Event<PrizeClaimedEventDTO>.GetEventABI().CreateFilterInput();
                prizeClaimed.Address = new string[] { contractAddress };
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
                                    + $"PrizeClaimed订阅异常:{ex}"
                            );
                            _isRunning = false;
                            await Task.Delay(2000);
                            await StartAsync(nodeUrl, contractAddress, chain_id);
                        }
                    );

                await _subscription.SubscribeAsync(prizeClaimed);
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                        + $"PrizeClaimed启动异常:{ex} - Chain ID: {chain_id}"
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
                DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "PrizeClaimed监听到了！"
            );
            // decode the log into a typed event log
            var decoded = Event<PrizeClaimedEventDTO>.DecodeEvent(log);
            using (var scope = _serviceProvider.CreateScope())
            {
                var _masterDbContext =
                    scope.ServiceProvider.GetRequiredService<MySqlMasterDbContext>();
                var card = _masterDbContext
                    .card_type.Where(a =>
                        a.type == decoded.Event.BoxType && a.chain_id == chain_id && a.state == 1
                    )
                    .FirstOrDefault();
                var chainToken = _masterDbContext
                    .chain_tokens.Where(a =>
                        a.token_address.Equals(card.token) && a.chain_id == chain_id
                    )
                    .FirstOrDefault();
                var decimals_num = (double)Math.Pow(10, chainToken.decimals);
                var prize = (double)decoded.Event.Prize / decimals_num;
                _masterDbContext.card_opened.Add(
                    new card_opened()
                    {
                        buyer = decoded.Event.User,
                        card_name = card.name,
                        card_type = card.type,
                        chain_id = chain_id,
                        create_time = DateTime.Now,
                        creator = "system",
                        contract = log.Address,
                        img = prize > 0 ? card.img_win : card.img_fail,
                        price = card.price,
                        token = card.token,
                        wining = prize,
                    }
                );
                var cardNotOpened = _masterDbContext
                    .card_not_opened.Where(a =>
                        a.buyer.Equals(decoded.Event.User)
                        && a.card_type.Equals(card.type)
                        && a.contract.Equals(log.Address)
                        && a.token.Equals(chainToken.token_address)
                    )
                    .FirstOrDefault();
                cardNotOpened.amount -= 1;
                cardNotOpened.updater = "system";
                cardNotOpened.update_time = DateTime.Now;

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
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + $"PrizeClaimed清理资源异常:{ex}"
                );
            }
        }
    }
}
