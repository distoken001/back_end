using CommonLibrary.Common.Common;

namespace DeMarketAPI.Common.Model.HttpApiModel.RequestModel
{
    public class TokenQueryRequest
    {
        public int pageSize { get; set; } = 10;
        public int pageIndex { get; set; } = 1;
    }
}
