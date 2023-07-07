using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using deMarketService.Common.Model;

namespace deMarketService.Common.Model.HttpApiModel.ResponseModel
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
        public int status { get; set; }

        /**
         * 创建时间
         */
        public DateTime create_time { get; set; }

        /**
         * 修改时间
         */
        public DateTime update_time { get; set; }
        /// <summary>
        /// 费率
        /// </summary>
        public decimal rate { get; set; }
        /// <summary>
        /// 店铺名称
        /// </summary>
        public string store_name { get; set; }
        /// <summary>
        /// 社区名称
        /// </summary>
        public string club_name { get; set; }
        /// <summary>
        /// 支持哪几种发布商品的方式1:个人 2:店铺 4:社区 注：相加为权限
        /// </summary>
        public int type { get; set; }

    }
}
