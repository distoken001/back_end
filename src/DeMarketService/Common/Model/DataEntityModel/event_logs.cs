using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace deMarketService.Common.Model.DataEntityModel
{
    [Table("event_logs")]
    public class event_logs
    {
        [Key]
        public int id { get; set; }

        /**
         * 事件名
         */
        public String eventName { get; set; }
        /**
         * 调用人
         */
        public String operater { get; set; }

        /**
         * 订单编号
         */
        public String orderId { get; set; }

        public String hash { get; set; }
        /**
         * 发送数据
         */
        public String data { get; set; }

        /**
         * 0:有效，1：无效
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

    }
}
