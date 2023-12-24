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
    public class BoxMintService : IHostedService
    {
        private readonly IConfiguration _configuration;
        private readonly IBoxMinted _cardPurchased;
        public BoxMintService(IConfiguration configuration, IBoxMinted cardPurchased)
        {
            _configuration = configuration;
            _cardPurchased = cardPurchased;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                Console.WriteLine("BoxPurchasedService启动啦！");

                //if (!string.IsNullOrEmpty(_configuration["Polygon:Contract_Box"]))
                //{
                //    _ = _cardPurchased.StartAsync(_configuration["Polygon:WSS_URL"], _configuration["Polygon:Contract_Box"], ChainEnum.Polygon);
                //}
                //if (!string.IsNullOrEmpty(_configuration["ARB:Contract_Box"]))
                //{
                //    _ = _cardPurchased.StartAsync(_configuration["ARB:WSS_URL"], _configuration["ARB:Contract_Box"], ChainEnum.Arbitrum);
                //}
                if (_configuration["Env"] == "prod")
                {
                    if (!string.IsNullOrEmpty(_configuration["BSC:Contract_Box"]))
                    {
                        _ = _cardPurchased.StartAsync(_configuration["BSC:WSS_URL"], _configuration["BSC:Contract_Box"], ChainEnum.Bsc);
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(_configuration["BSC:Contract_Box"]))
                    {
                        _ = _cardPurchased.StartAsync(_configuration["BSC:WSS_URL"], _configuration["BSC:Contract_Box"], ChainEnum.Bsc);
                    }
                    if (!string.IsNullOrEmpty(_configuration["OPGoerli:Contract_Box"]))
                    {
                        ChainEnum chain_id = ChainEnum.OptimisticGoerli;
                        _ = _cardPurchased.StartAsync(_configuration["OPGoerli:WSS_URL"], _configuration["OPGoerli:Contract_Box"], chain_id);
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

