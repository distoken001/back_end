using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace deMarketService.Common.Model.HttpApiModel.ResponseModel
{
    public class LoginResponse
    {
        public string token { get; set; }
        public string avatar { get; set; }
        public string nick_name { get; set; }
        public string email { get; set; }
        public bool is_first { get; set; }
        public int[] nfts { get; set; }
    }
}