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
using ListenService.Model;
using Nethereum.JsonRpc.Client;
using CommonLibrary.Model.DataEntityModel;
using CommonLibrary.Common.Common;
using ListenService.Repository.Interfaces;
using ListenService.Repository.Implements;

namespace ListenService.Service
{
    public class CardPurchasedService : IHostedService
    {
        private readonly IConfiguration _configuration;
        private readonly ICardPurchased _cardPurchased;
        public CardPurchasedService(IConfiguration configuration, ICardPurchased cardPurchased)
        {
            _configuration = configuration;
            _cardPurchased = cardPurchased;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                Console.WriteLine("CardPurchasedService启动啦！");

                //if (!string.IsNullOrEmpty(_configuration["Polygon:Contract_ScratchCard"]))
                //{
                //    _ = _cardPurchased.StartAsync(_configuration["Polygon:WSS_URL"], _configuration["Polygon:Contract_ScratchCard"], ChainEnum.Polygon);
                //}
                //if (!string.IsNullOrEmpty(_configuration["ARB:Contract_ScratchCard"]))
                //{
                //    _ = _cardPurchased.StartAsync(_configuration["ARB:WSS_URL"], _configuration["ARB:Contract_ScratchCard"], ChainEnum.Arbitrum);
                //}
                if (_configuration["Env"] == "prod")
                {
                    if (!string.IsNullOrEmpty(_configuration["BSC:Contract_ScratchCard"]))
                    {
                        _ = _cardPurchased.StartAsync(_configuration["BSC:WSS_URL"], _configuration["BSC:Contract_ScratchCard"], ChainEnum.Bsc);
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(_configuration["OPGoerli:Contract_ScratchCard"]))
                    {
                        ChainEnum chain_id = ChainEnum.OptimisticGoerli;
                        _ = _cardPurchased.StartAsync(_configuration["OPGoerli:WSS_URL"], _configuration["OPGoerli:Contract_ScratchCard"], chain_id);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

    }
}

