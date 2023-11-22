using System;
using CommonLibrary.Common.Common;

namespace DeMarketAPI.Common.Model.HttpApiModel.ResponseModel
{
    public class ChainTokenViewModel
    {
        public long id { get; set; }

        public ChainEnum chain_id { get; set; }

        public string token_name { get; set; }

        public string token_address { get; set; }

        public string icon { get; set; }

        public int decimals { get; set; }
    }
}
