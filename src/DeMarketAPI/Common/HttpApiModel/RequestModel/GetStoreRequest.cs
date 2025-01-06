namespace DeMarketAPI.Common.Model.HttpApiModel.RequestModel
{
    public class GetStoreRequest
    {
        /// <summary>
        /// 店铺名称
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// 店铺address（eth地址）
        /// </summary>
        public string address { get; set; }

        /// <summary>
        /// 1个人 2店铺 4社区
        /// </summary>
        public int type { get; set; }

        /// <summary>
        /// 页大小
        /// </summary>
        public int pageSize { get; set; } = 10;

        /// <summary>
        /// 第几页
        /// </summary>
        public int pageIndex { get; set; } = 1;
    }
}