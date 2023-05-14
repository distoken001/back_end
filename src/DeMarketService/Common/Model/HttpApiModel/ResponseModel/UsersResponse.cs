using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using deMarketService.Common.Model;

namespace deMarketService.Controllers
{
    public class UsersResponse
    {
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
        public String nick_name { get; set; }

        /**
         * 状态，0：有效，1无效
         */
        public ChainEnum status { get; set; }

        /**
         * 创建时间
         */
        public DateTime create_time { get; set; }

        /**
         * 修改时间
         */
        public DateTime update_time { get; set; }

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
        public int parent_id;

        /*
         * 用户登录ip
         */
        public String ip { get; set; }
        /// <summary>
        /// 链id
        /// </summary>
        public ChainEnum chain_id { get; set; }
        /// <summary>
        /// 链名称
        /// </summary>
        public string chain_name { get { if (chain_id == ChainEnum.Avalanche) { return "Avalanche C-Chain"; } else if (chain_id == ChainEnum.Polygon) { return "Polygon(Matic)"; } else return chain_id.ToString(); } }
    }
}
