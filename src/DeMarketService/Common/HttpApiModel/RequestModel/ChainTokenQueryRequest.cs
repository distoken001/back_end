using CommonLibrary.Common.Model;

namespace deMarketService.Common.Model.HttpApiModel.RequestModel
{
    public class ChainTokenQueryRequest
    {
        public int pageSize { get; set; } = 10;
        public int pageIndex { get; set; } = 1;
        public ChainEnum chainId { get; set; }
    }
}
