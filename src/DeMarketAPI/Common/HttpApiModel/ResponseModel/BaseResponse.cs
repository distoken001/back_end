namespace DeMarketAPI.Model.HttpApiModel.ResponseModel
{
    public class BaseResponse<T>
    {
        /// <summary>
        /// 返回码
        /// 0：识别成功
        /// 其他：识别失败
        /// </summary>
        public int code { get; set; }
        /// <summary>
        /// 请求结果描述
        /// </summary>
        public string msg { get; set; }
        public string bizSeqNo { get; set; }
        public string transactionTime { get; set; }
        public T result { get; set; }
    }
}
