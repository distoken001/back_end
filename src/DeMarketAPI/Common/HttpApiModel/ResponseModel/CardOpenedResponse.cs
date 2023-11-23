using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using CommonLibrary.Common.Common;

namespace DeMarketAPI.Common.Model.HttpApiModel.ResponseModel
{
    public class CardOpenedResponse
    {
        public int id { get; set; }
        public string card_type { get; set; }
        public string card_name { get; set; }
        public double price { get; set; }
        public double wining { get; set; }
        public DateTime create_time { get; set; }
        public DateTime? update_time { get; set; }
        public string updater { get; set; }
        public string creator { get; set; }
        public string buyer { get; set; }
        public ChainEnum chain_id { get; set; }
        public string contract { get; set; }
        public string token { get; set; }
        public string img { get; set; }
        public ChainTokenViewModel token_des { get; set; }
        /// <summary>
        /// 链名称
        /// </summary>
        public string chain_name { get { if (chain_id == ChainEnum.Bsc) { return "BNB Chain"; } else return chain_id.ToString(); } }
    }
}
