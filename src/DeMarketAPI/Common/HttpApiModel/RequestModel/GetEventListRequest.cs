namespace DeMarketAPI.Common.Model.HttpApiModel.RequestModel
{
    public class GetEventListRequest
    {
        public int pageSize { get; set; } = 10;
        public int pageIndex { get; set; } = 1;
        ///// <summary>
        ///// 链id
        ///// </summary>
        //public int chain_id { get; set; }
    }
}