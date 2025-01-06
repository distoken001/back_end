using CommonLibrary.Common.Common;

namespace DeMarketAPI.Common.Model.HttpApiModel.RequestModel
{
    public class RefreshRequest
    {
        /// <summary>
        /// 链id
        /// </summary>
        public ChainEnum chain_id { get; set; }
    }
}