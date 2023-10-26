namespace deMarketService.Common.Model.HttpApiModel.ResponseModel
{
    public class TxCosViewResponse
    {
        public string ButketName { get; set; }

        public string Region { get; set; }     

        public TxCosViewResponse() {
            this.ButketName = "demarket-1303108648";
            this.Region = "ap-beijing";
        }
    }
}
