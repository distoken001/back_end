using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DeMarketAPI.Common.Model.HttpApiModel.RequestModel
{
    public class LoginRequest
    {
        /// <summary>
        /// 签名
        /// </summary>
        public String signature { get; set; }
        /// <summary>
        /// 邀请人
        /// </summary>
        public String parentAddress { get; set; }
        /// <summary>
        /// 地址
        /// </summary>
        public String address { get; set; }

    }
}
