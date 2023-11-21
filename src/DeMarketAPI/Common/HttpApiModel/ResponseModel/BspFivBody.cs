namespace DeMarketAPI.Model.HttpApiModel.ResponseModel
{
    public class BspFivBody
    {
        /// <summary>
        /// 请求是否验证成功标识
        /// 00：成功
        /// 其它：失败
        /// </summary>
        public string authCode { get; set; }
        /// <summary>
        /// 验证信息
        /// </summary>
        public string authMessage { get; set; }
    }
}
