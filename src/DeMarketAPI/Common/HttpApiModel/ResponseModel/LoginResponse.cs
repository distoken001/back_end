using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DeMarketAPI.Common.Model.HttpApiModel.ResponseModel
{
    public class LoginResponse
    {
        /// <summary>
        /// token
        /// </summary>
        public string token { get; set; }

        /// <summary>
        /// 头像
        /// </summary>
        public string avatar { get; set; }

        /// <summary>
        /// 昵称
        /// </summary>
        public string nick_name { get; set; }

        /// <summary>
        /// 邮箱
        /// </summary>
        public string email { get; set; }

        /// <summary>
        /// 是否第一次登录
        /// </summary>
        public bool is_first { get; set; }

        /// <summary>
        /// 持有的nft
        /// </summary>
        public int[] nfts { get; set; }
    }
}