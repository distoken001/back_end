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
using ListenService.Repository.Implements;

namespace ListenService.Service
{
    public class BoxTypeRemovedService : IHostedService
    {
        private readonly IConfiguration _configuration;
        private readonly IBoxTypeRemoved _cardTypeRemoved;
        public BoxTypeRemovedService(IConfiguration configuration, IBoxTypeRemoved cardTypeRemoved)
        {
            _configuration = configuration;
            _cardTypeRemoved = cardTypeRemoved;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                Console.WriteLine("BoxTypeRemovedService启动啦！");
                //if (!string.IsNullOrEmpty(_configuration["Polygon:Contract_ScratchBox"]))
                //{
                //    _ = _cardTypeRemoved.StartAsync(_configuration["Polygon:WSS_URL"], _configuration["Polygon:Contract_ScratchBox"], ChainEnum.Polygon);
                //}
                //if (!string.IsNullOrEmpty(_configuration["ARB:Contract_ScratchBox"]))
                //{
                //    _ = _cardTypeRemoved.StartAsync(_configuration["ARB:WSS_URL"], _configuration["ARB:Contract_ScratchBox"], ChainEnum.Arbitrum);
                //}
                if (_configuration["Env"] == "prod")
                {
                    if (!string.IsNullOrEmpty(_configuration["BSC:Contract_ScratchBox"]))
                    {
                        _ = _cardTypeRemoved.StartAsync(_configuration["BSC:WSS_URL"], _configuration["BSC:Contract_ScratchBox"], ChainEnum.Bsc);
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(_configuration["OPGoerli:Contract_ScratchBox"]))
                    {
                        ChainEnum chain_id = ChainEnum.OptimisticGoerli;
                        _ = _cardTypeRemoved.StartAsync(_configuration["OPGoerli:WSS_URL"], _configuration["OPGoerli:Contract_ScratchBox"], chain_id);
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

