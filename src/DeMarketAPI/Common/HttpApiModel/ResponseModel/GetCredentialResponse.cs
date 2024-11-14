namespace DeMarketAPI.Common.Model.HttpApiModel.ResponseModel
{
    public class GetCredentialResponse
    {
        public Credential Credentials { get; set; }
        public long ExpiredTime { get; set; }
        public string Expiration { get; set; }
        public string RequestId { get; set; }
        public long StartTime { get; set; }
    }

    public class Credential
    {
        public string Token { get; set; }
        public string TmpSecretId { get; set; }
        public string TmpSecretKey { get; set; }
    }
}