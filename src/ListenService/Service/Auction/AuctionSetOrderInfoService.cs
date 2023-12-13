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

    public class AuctionSetOrderInfoService : IHostedService
    {
        private readonly IConfiguration _configuration;
        private readonly IAuctionSetOrderInfo _auctionSetOrderInfo;
        public AuctionSetOrderInfoService(IConfiguration configuration, IAuctionSetOrderInfo auctionSetOrderInfo)
        {
            _configuration = configuration;
            _auctionSetOrderInfo = auctionSetOrderInfo;
        }
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
               
                if (_configuration["Env"] == "prod")
                {
                    if (!string.IsNullOrEmpty(_configuration["Polygon:Contract_Auction"]))
                    {
                        _ = _auctionSetOrderInfo.StartAsync(_configuration["Polygon:WSS_URL"], _configuration["Polygon:HTTPS_URL"] ,_configuration["Polygon:Contract_Auction"], ChainEnum.Polygon);
                    }
                    if (!string.IsNullOrEmpty(_configuration["ARB:Contract_Auction"]))
                    {
                        _ = _auctionSetOrderInfo.StartAsync(_configuration["ARB:WSS_URL"], _configuration["ARB:HTTPS_URL"], _configuration["ARB:Contract_Auction"], ChainEnum.Arbitrum);
                    }
                    if (!string.IsNullOrEmpty(_configuration["BSC:Contract_Auction"]))
                    {
                        _ = _auctionSetOrderInfo.StartAsync(_configuration["BSC:WSS_URL"], _configuration["BSC:HTTPS_URL"], _configuration["BSC:Contract_Auction"], ChainEnum.Bsc);
                    }

                    if (!string.IsNullOrEmpty(_configuration["OP:Contract_Auction"]))
                    {
                        _ = _auctionSetOrderInfo.StartAsync(_configuration["OP:WSS_URL"], _configuration["OP:HTTPS_URL"], _configuration["OP:Contract_Auction"], ChainEnum.Optimism);
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(_configuration["OPGoerli:Contract_Auction"]))
                    {
                        _ = _auctionSetOrderInfo.StartAsync(_configuration["OPGoerli:WSS_URL"], _configuration["OPGoerli:HTTPS_URL"], _configuration["OPGoerli:Contract_Auction"], ChainEnum.OptimisticGoerli);
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
            throw new NotImplementedException();
        }
    }
}

