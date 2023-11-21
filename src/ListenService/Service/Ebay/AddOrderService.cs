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

    public class AddOrderService : IHostedService
    {
        private readonly IConfiguration _configuration;
        private readonly IAddOrder _addOrder;
        public AddOrderService(IConfiguration configuration, IAddOrder addOrder)
        {
            _configuration = configuration;
            _addOrder = addOrder;
        }
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                ChainEnum chain_id = ChainEnum.OptimisticGoerli;
                if (_configuration["Env"] == "prod")
                {
                    chain_id = ChainEnum.Optimism;
                }
                await _addOrder.StartAsync(_configuration["OP:WSS_URL"], _configuration["OP:Contract_Ebay"], chain_id);
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

