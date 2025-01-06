using CommonLibrary.Common.Common;
using ListenService.Repository.Interfaces;
using System.Reactive.Linq;

namespace ListenService.Service
{
    public class BoxTypeRemovedService : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly IBoxTypeRemoved _cardTypeRemoved;

        public BoxTypeRemovedService(IConfiguration configuration, IBoxTypeRemoved cardTypeRemoved)
        {
            _configuration = configuration;
            _cardTypeRemoved = cardTypeRemoved;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            try
            {
                Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "BoxTypeRemovedService启动啦！");
                //if (!string.IsNullOrEmpty(_configuration["Polygon:Contract_Box"]))
                //{
                //    _ = _cardTypeRemoved.StartAsync(_configuration["Polygon:WSS_URL"], _configuration["Polygon:Contract_Box"], ChainEnum.Polygon);
                //}
                //if (!string.IsNullOrEmpty(_configuration["ARB:Contract_Box"]))
                //{
                //    _ = _cardTypeRemoved.StartAsync(_configuration["ARB:WSS_URL"], _configuration["ARB:Contract_Box"], ChainEnum.Arbitrum);
                //}
                if (_configuration["Env"] == "prod")
                {
                    if (!string.IsNullOrEmpty(_configuration["BSC:Contract_Box"]))
                    {
                        _ = _cardTypeRemoved.StartAsync(_configuration["BSC:WSS_URL"], _configuration["BSC:Contract_Box"], ChainEnum.Bsc);
                    }
                }
                else
                {
                    //if (!string.IsNullOrEmpty(_configuration["BSC:Contract_Box"]))
                    //{
                    //    _ = _cardTypeRemoved.StartAsync(_configuration["BSC:WSS_URL"], _configuration["BSC:Contract_Box"], ChainEnum.Bsc);
                    //}
                    //if (!string.IsNullOrEmpty(_configuration["OPGoerli:Contract_Box"]))
                    //{
                    //    ChainEnum chain_id = ChainEnum.OptimisticGoerli;
                    //    _ = _cardTypeRemoved.StartAsync(_configuration["OPGoerli:WSS_URL"], _configuration["OPGoerli:Contract_Box"], chain_id);
                    //}
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