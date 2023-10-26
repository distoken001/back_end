namespace deMarketService.Model.HttpApiModel.ResponseModel
{
    public class AccessTokenResponse
    {
        public string code { get; set; }
        public string msg { get; set; }
        public string transactionTime { get; set; }
        public string access_token { get; set; }
        public string expire_time { get; set; }
        public int expire_in { get; set; }
    }
}
