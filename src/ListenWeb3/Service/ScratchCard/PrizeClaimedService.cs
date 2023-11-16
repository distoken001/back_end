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
using Microsoft.VisualBasic;
using Nethereum.Contracts.Standards.ERC20.TokenList;

namespace ListenWeb3.Service.ScratchCard
{
    public class PrizeClaimedService : IHostedService
    {
        private readonly IConfiguration _configuration;
        private readonly MySqlMasterDbContext _masterDbContext;
        public PrizeClaimedService(IConfiguration configuration, MySqlMasterDbContext mySqlMasterDbContext)
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

                var client = new StreamingWebSocketClient(nodeUrl);

                var prizeClaimed = Event<PrizeClaimedEventDTO>.GetEventABI().CreateFilterInput();

                var subscription = new EthLogsObservableSubscription(client);
                // attach a handler for Transfer event logs
                subscription.GetSubscriptionDataResponsesAsObservable().Subscribe(log =>
                {
                    try
                    {
                        // decode the log into a typed event log
                        var decoded = Event<PrizeClaimedEventDTO>.DecodeEvent(log);
                        if (decoded != null)
                        {
                            ChainEnum chain_id = ChainEnum.OptimisticGoerli;
                            if (_configuration["Env"] == "prod")
                            {
                                chain_id = ChainEnum.Optimism;
                            }
                            var card = _masterDbContext.card_type.Where(a => a.type == decoded.Event.CardType).FirstOrDefault();
                            var chainToken = _masterDbContext.chain_tokens.Where(a => a.token_address.Equals(card.token) && a.chain_id == chain_id).FirstOrDefault();
                            var decimals_num = (double)Math.Pow(10, chainToken.decimals);
                            var prize = (double)decoded.Event.Prize / decimals_num;
                            _masterDbContext.card_opened.Add(new card_opened() { buyer = decoded.Event.User, card_name = card.name, card_type = card.type, chain_id = chain_id, create_time = DateTime.Now, creator = "system", contract = log.Address, img = card.img, price = card.price, token = card.token, wining = prize });
                            var cardNotOpened = _masterDbContext.card_not_opened.Where(a => a.buyer.Equals(decoded.Event.User) && a.card_type.Equals(card.type) && a.contract.Equals(log.Address) && a.token.Equals(chainToken.token_address)).FirstOrDefault();
                            cardNotOpened.amount -= 1;
                            cardNotOpened.updater = "system";
                            cardNotOpened.update_time = DateTime.Now;

                            _masterDbContext.SaveChanges();
                        }
                        else
                        {
                            Console.WriteLine("PrizeClaimedService:Found not standard log");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("PrizeClaimedService: Log Address: " + log.Address + " is not a standard log:", ex.Message);
                    }
                });
                // open the web socket connection
                await client.StartAsync();

                // begin receiving subscription data
                // data will be received on a background thread
                await subscription.SubscribeAsync(prizeClaimed);


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

