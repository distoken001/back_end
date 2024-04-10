using System;
using CommonLibrary.Common.Common;

namespace DeMarketAPI.Common.Model.HttpApiModel.ResponseModel
{
    public class TokenViewModel
    {
        public string token_name { get; set; }
        public string icon { get; set; }
        public DateTime create_time { get; set; }
        public int weight { get; set; }
        public int status { get; set; }
    }
}
