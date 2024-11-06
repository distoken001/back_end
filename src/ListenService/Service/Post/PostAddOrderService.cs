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

    public class PostAddOrderService : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly IPostAddOrder _addOrder;
        public PostAddOrderService(IConfiguration configuration, IPostAddOrder addOrder)
        {
            _configuration = configuration;
            _addOrder = addOrder;
        }
        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            try
            {
               
                if (_configuration["Env"] == "prod")
                {
                    //if (!string.IsNullOrEmpty(_configuration["Polygon:Contract_Post"]))
                    //{
                    //    _ = _addOrder.StartAsync(_configuration["Polygon:WSS_URL"], _configuration["Polygon:HTTPS_URL"], _configuration["Polygon:Contract_Post"], ChainEnum.Polygon);
                    //}
                    //if (!string.IsNullOrEmpty(_configuration["ARB:Contract_Post"]))
                    //{
                    //    _ = _addOrder.StartAsync(_configuration["ARB:WSS_URL"], _configuration["ARB:HTTPS_URL"], _configuration["ARB:Contract_Post"], ChainEnum.Arbitrum);
                    //}
                    if (!string.IsNullOrEmpty(_configuration["BSC:Contract_Post"]))
                    {
                        _ = _addOrder.StartAsync(_configuration["BSC:WSS_URL"], _configuration["BSC:HTTPS_URL"], _configuration["BSC:Contract_Post"], ChainEnum.Bsc);
                    }
                    //if (!string.IsNullOrEmpty(_configuration["OP:Contract_Post"]))
                    //{
                    //    _ = _addOrder.StartAsync(_configuration["OP:WSS_URL"], _configuration["OP:HTTPS_URL"], _configuration["OP:Contract_Post"], ChainEnum.Optimism);
                    //}
                }
                else
                {
                    //if (!string.IsNullOrEmpty(_configuration["BSC:Contract_Post"]))
                    //{
                    //    _ = _addOrder.StartAsync(_configuration["BSC:WSS_URL"], _configuration["BSC:HTTPS_URL"], _configuration["BSC:Contract_Post"], ChainEnum.Bsc);
                    //}
                    if (!string.IsNullOrEmpty(_configuration["Sepolia:Contract_Post"]))
                    {
                        _ = _addOrder.StartAsync(_configuration["Sepolia:WSS_URL"], _configuration["Sepolia:HTTPS_URL"], _configuration["Sepolia:Contract_Post"], ChainEnum.Sepolia);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
      
    }
}

