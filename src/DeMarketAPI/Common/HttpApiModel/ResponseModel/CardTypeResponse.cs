using CommonLibrary.Common.Common;

namespace DeMarketAPI.Common.Model.HttpApiModel.ResponseModel
{
    public class CardTypeResponse
    {
        public string type { get; set; }
        public string name { get; set; }
        public double price { get; set; }
        public double max_prize { get; set; }
        public string token { get; set; }
        public int winning_probability { get; set; }
        public int max_prize_probability { get; set; }
        public ChainEnum chain_id { get; set; }
        public string img { get; set; }
        public int state { get; set; }
        public ChainTokenViewModel token_des { get; set; }
    }
}