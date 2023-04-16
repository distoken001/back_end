using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace deMarketService.Common.Model.DataEntityModel
{
    [Table("users")]
    public class users
    {
        [Key]
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
        public int status { get; set; }

        /**
         * 创建时间
         */
        public DateTime createTime { get; set; }

        /**
         * 修改时间
         */
        public DateTime updateTime { get; set; }

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
        public int parentId;

        /*
         * 用户登录ip
         */
        public String ip { get; set; }
    }
}
