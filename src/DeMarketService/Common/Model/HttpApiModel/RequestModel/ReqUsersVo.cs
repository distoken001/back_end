using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace deMarketService.Common.Model.HttpApiModel.RequestModel
{
    public class ReqUsersVo
    {
        public String signature { get; set; }
        public String token;
        public String msg;
        public String inviteCode { get; set; }
        public String parentAddress { get; set; }
        public String thirdName { get; set; }

        public int id { get; set; }

        /**
         * 钱包地址
         */
        public String address { get; set; }

        /**
         * 头像
         */
        public String avatar { get; set; }

        /**
         * 邮箱
         */
        public String email { get; set; }

        /**
         * 昵称
         */
        public String nickName { get; set; }


        /**
         * 状态，0：有效，1无效
         */
        public ChainEnum status { get; set; }

        /**
         * 创建时间
         */

        public string createTime { get; set; }

        /**
         * 修改时间
         */
        public string updateTime { get; set; }

        /**
         * 修改人
         */
        public String updater { get; set; }

        /**
         * 创建人
         */
        public String creator { get; set; }

        /**
         * 上级id
         */

        public int parentId { get; set; }

        public String ip { get; set; } //用户登录ip
        /// <summary>
        /// 链id
        /// </summary>
        public ChainEnum chain_id { get; set; }

    }
}
