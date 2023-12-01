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
using Microsoft.VisualBasic;
using Nethereum.Contracts.Standards.ERC20.TokenList;
using ListenService.Repository.Interfaces;

namespace ListenService.Service
{
    public class CardGiftedService : IHostedService
    {
        private readonly IConfiguration _configuration;
        private readonly ICardGifted _cardGifted;
        public CardGiftedService(IConfiguration configuration, ICardGifted cardGifted)
        {
            _configuration = configuration;
            _cardGifted = cardGifted;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                Console.WriteLine("CardGiftedService启动啦！");

                if (!string.IsNullOrEmpty(_configuration["Polygon:Contract_ScratchCard"]))
                {
                    _cardGifted.StartAsync(_configuration["Polygon:WSS_URL"], _configuration["Polygon:Contract_ScratchCard"],ChainEnum.Polygon);
                }
                if (!string.IsNullOrEmpty(_configuration["ARB:Contract_ScratchCard"]))
                {
                    _cardGifted.StartAsync(_configuration["ARB:WSS_URL"], _configuration["ARB:Contract_ScratchCard"], ChainEnum.Arbitrum);
                }
                if (!string.IsNullOrEmpty(_configuration["BSC:Contract_ScratchCard"]))
                {
                    _cardGifted.StartAsync(_configuration["BSC:WSS_URL"], _configuration["BSC:Contract_ScratchCard"], ChainEnum.Bsc);
                }

                if (!string.IsNullOrEmpty(_configuration["OP:Contract_ScratchCard"]))
                {
                    ChainEnum chain_id = ChainEnum.OptimisticGoerli;
                    if (_configuration["Env"] == "prod")
                    {
                        chain_id = ChainEnum.Optimism;
                    }
                    _cardGifted.StartAsync(_configuration["OP:WSS_URL"], _configuration["OP:Contract_ScratchCard"], chain_id);
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

