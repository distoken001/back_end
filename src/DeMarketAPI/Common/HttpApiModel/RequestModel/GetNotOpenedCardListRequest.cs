using CommonLibrary.Common.Common;

namespace DeMarketAPI.Common.Model.HttpApiModel.RequestModel
{
    public class GetNotOpenedCardListRequest
    {
        public int pageSize { get; set; } = 10;
        public int pageIndex { get; set; } = 1;

        ///// <summary>
        ///// 链id
        ///// </summary>
        public ChainEnum chain_id { get; set; }
    }
}