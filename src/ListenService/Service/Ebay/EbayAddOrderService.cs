using CommonLibrary.Common.Common;
using ListenService.Repository.Interfaces;
using System.Reactive.Linq;

namespace ListenService.Service
{
    public class EbayAddOrderService : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly IEbayAddOrder _addOrder;

        public EbayAddOrderService(IConfiguration configuration, IEbayAddOrder addOrder)
        {
            _configuration = configuration;
            _addOrder = addOrder;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            try
            {
                //Console.WriteLine("启动EbayAddOrderService！");
                if (_configuration["Env"] == "prod")
                {
                    //if (!string.IsNullOrEmpty(_configuration["Polygon:Contract_Ebay"]))
                    //{
                    //    _ = _addOrder.StartAsync(_configuration["Polygon:WSS_URL"], _configuration["Polygon:HTTPS_URL"], _configuration["Polygon:Contract_Ebay"], ChainEnum.Polygon);
                    //}
                    //if (!string.IsNullOrEmpty(_configuration["ARB:Contract_Ebay"]))
                    //{
                    //    _ = _addOrder.StartAsync(_configuration["ARB:WSS_URL"], _configuration["ARB:HTTPS_URL"], _configuration["ARB:Contract_Ebay"], ChainEnum.Arbitrum);
                    //}
                    if (!string.IsNullOrEmpty(_configuration["BSC:Contract_Ebay"]))
                    {
                        _ = _addOrder.StartAsync(_configuration["BSC:WSS_URL"], _configuration["BSC:HTTPS_URL"], _configuration["BSC:Contract_Ebay"], ChainEnum.Bsc);
                    }
                    //if (!string.IsNullOrEmpty(_configuration["OP:Contract_Ebay"]))
                    //{
                    //    _ = _addOrder.StartAsync(_configuration["OP:WSS_URL"], _configuration["OP:HTTPS_URL"], _configuration["OP:Contract_Ebay"], ChainEnum.Optimism);
                    //}
                }
                else
                {
                    //if (!string.IsNullOrEmpty(_configuration["BSC:Contract_Ebay"]))
                    //{
                    //    _ = _addOrder.StartAsync(_configuration["BSC:WSS_URL"], _configuration["BSC:HTTPS_URL"], _configuration["BSC:Contract_Ebay"], ChainEnum.Bsc);
                    //}
                    if (!string.IsNullOrEmpty(_configuration["Sepolia:Contract_Ebay"]))
                    {
                        _ = _addOrder.StartAsync(_configuration["Sepolia:WSS_URL"], _configuration["Sepolia:HTTPS_URL"], _configuration["Sepolia:Contract_Ebay"], ChainEnum.Sepolia);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            var tcs = new TaskCompletionSource<bool>();
            cancellationToken.Register(s => ((TaskCompletionSource<bool>)s).SetResult(true), tcs);
            await tcs.Task;
        }
    }
}