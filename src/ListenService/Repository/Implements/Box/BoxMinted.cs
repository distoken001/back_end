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
    public class BoxMinted : IBoxMinted
    {
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;
        private readonly IDatabase _redisDb;
        private readonly ClientManage _clientManage;
        private volatile bool _isRunning = false;
        private EthLogsObservableSubscription _subscription;

        public BoxMinted(
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
                        + "BoxMinted已经在运行中，跳过启动："
                        + chain_id
                );
                return;
            }

            _isRunning = true;
            Console.WriteLine(
                DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "BoxMinted程序启动：" + chain_id
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

                var cardPurchased = Event<BoxMintedEventDTO>.GetEventABI().CreateFilterInput();
                cardPurchased.Address = new string[] { contractAddress };
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
                                    + $"BoxMinted订阅异常:{ex}"
                            );
                            _isRunning = false;
                            await Task.Delay(2000);
                            await StartAsync(nodeUrl, contractAddress, chain_id);
                        }
                    );

                await _subscription.SubscribeAsync(cardPurchased);
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                        + $"BoxMinted启动异常:{ex} - Chain ID: {chain_id}"
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

            Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "BoxMinted监听到了！");
            // decode the log into a typed event log
            var decoded = Event<BoxMintedEventDTO>.DecodeEvent(log);
            using (var scope = _serviceProvider.CreateScope())
            {
                var _masterDbContext =
                    scope.ServiceProvider.GetRequiredService<MySqlMasterDbContext>();
                var card = _masterDbContext
                    .card_type.Where(a =>
                        a.type == decoded.Event.BoxType && a.chain_id == chain_id && a.state == 1
                    )
                    .FirstOrDefault();
                var token = _masterDbContext
                    .chain_tokens.Where(a =>
                        a.token_address.Equals(card.token) && a.chain_id == card.chain_id
                    )
                    .FirstOrDefault();
                var cardNotOpened = _masterDbContext
                    .card_not_opened.Where(a =>
                        a.buyer.Equals(decoded.Event.User)
                        && a.card_type.Equals(card.type)
                        && a.contract.Equals(log.Address)
                        && a.token.Equals(token.token_address)
                    )
                    .FirstOrDefault();
                if (cardNotOpened != null)
                {
                    cardNotOpened.amount += (int)decoded.Event.NumberOfBoxs;
                    cardNotOpened.updater = "system";
                    cardNotOpened.update_time = DateTime.Now;
                }
                else
                {
                    var notOpened = new card_not_opened()
                    {
                        card_type = card.type,
                        card_name = card.name,
                        amount = (int)decoded.Event.NumberOfBoxs,
                        buyer = decoded.Event.User,
                        chain_id = chain_id,
                        contract = log.Address,
                        create_time = DateTime.Now,
                        creator = "system",
                        price = card.price,
                        token = card.token,
                        img = card.img,
                    };
                    _masterDbContext.card_not_opened.Add(notOpened);
                }
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
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + $"BoxMinted清理资源异常:{ex}"
                );
            }
        }
    }
}
